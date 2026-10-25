using Microsoft.AspNetCore.Mvc.Controllers;
using Handicraft.Domain.Entities;
using Handicraft.Infrastructure.Persistence.Data;
using Handicraft.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Handicraft.Api.Notifications;

public sealed class TelegramEventMiddleware
{
    private readonly RequestDelegate _next;

    public TelegramEventMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, TelegramEventQueue eventQueue, AppDbContext db, ILogger<TelegramEventMiddleware> logger)
    {
        var method = context.Request.Method;
        var isWrite = HttpMethods.IsPost(method) ||
                      HttpMethods.IsPut(method) ||
                      HttpMethods.IsPatch(method) ||
                      HttpMethods.IsDelete(method);
        var action = context.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>();
        var eventName = isWrite && action is not null ? GetEventName(action) : null;
        var notification = isWrite && action is not null ? GetAdminNotification(action) : null;
        string? registrationPhone = null;
        if (action is { ControllerName: "Auth", ActionName: "Register" })
        {
            context.Request.EnableBuffering();
            try
            {
                var request = await JsonSerializer.DeserializeAsync<RegisterDto>(
                    context.Request.Body,
                    new JsonSerializerOptions(JsonSerializerDefaults.Web));
                registrationPhone = request?.Phone;
            }
            catch (JsonException) { }
            finally
            {
                if (context.Request.Body.CanSeek)
                    context.Request.Body.Position = 0;
            }
        }

        await _next(context);

        if ((eventName is not null || notification is not null) && context.Response.StatusCode is >= 200 and < 300)
        {
            if (eventName is not null)
                eventQueue.TryEnqueue($"رویداد سایت: {eventName}");
            if (notification is not null)
            {
                try
                {
                    if (action is { ControllerName: "Auth", ActionName: "Register" } && !string.IsNullOrWhiteSpace(registrationPhone))
                    {
                        var registeredUser = await db.Users
                            .AsNoTracking()
                            .Where(user => user.Phone == registrationPhone)
                            .OrderByDescending(user => user.CreatedAt)
                            .Select(user => new { user.Id, user.Name, user.Phone })
                            .FirstOrDefaultAsync();

                        if (registeredUser is not null)
                        {
                            notification = notification with
                            {
                                Message = $"{registeredUser.Name} ({registeredUser.Phone}) برای فعال‌سازی حساب ثبت‌نام کرد.",
                                TargetUrl = $"/admin/users/{registeredUser.Id}"
                            };
                        }
                    }

                    db.AdminNotifications.Add(new AdminNotification
                    {
                        Id = Guid.NewGuid(),
                        Title = notification.Title,
                        Message = notification.Message,
                        TargetUrl = notification.TargetUrl,
                        CreatedAt = DateTime.UtcNow
                    });
                    await db.SaveChangesAsync();
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "Could not save an admin site notification.");
                }
            }
        }
    }

    private static AdminNotificationInfo? GetAdminNotification(ControllerActionDescriptor action) =>
        (action.ControllerName, action.ActionName) switch
        {
            ("Auth", "Register") => new("ثبت‌نام جدید", "یک کاربر برای دریافت دسترسی ثبت‌نام کرد.", "/admin/users"),
            ("Auth", "Login") => new("ورود کاربر", "یک کاربر با موفقیت وارد سایت شد.", "/admin"),
            ("Orders", "Create") => new("سفارش جدید", "یک سفارش ساخت اثر ثبت شد.", "/admin/orders"),
            ("Chat", "Send") => new("پیام تازه", "یک کاربر در سایت پیام فرستاد.", "/admin/messages"),
            _ => null
        };

    private sealed record AdminNotificationInfo(string Title, string Message, string TargetUrl);

    private static string? GetEventName(ControllerActionDescriptor action)
    {
        var controller = action.ControllerName;
        var method = action.ActionName;

        return (controller, method) switch
        {
            ("Auth", "Register") => "ثبت‌نام کاربر جدید",
            ("Orders", "Create") => "سفارش جدید ثبت شد",
            ("Orders", "UpdateByAdmin") => "وضعیت سفارش تغییر کرد",
            ("Orders", "UpdateByUser") => "اطلاعات سفارش توسط کاربر ویرایش شد",
            ("Users", "CreateManaged") => "حساب کاربری جدید ساخته شد",
            ("Users", "SetRole") => "نقش یک کاربر تغییر کرد",
            ("Users", "SetActive") => "وضعیت یک کاربر تغییر کرد",
            ("Users", "SetCategories") => "دسترسی دوره‌ای یک کاربر تغییر کرد",
            ("Users", "SetName") => "نام نمایشی یک کاربر تغییر کرد",
            ("CourseCategories", "Create") => "دسته‌بندی دوره ایجاد شد",
            ("Courses", "Create") => "دوره جدید ایجاد شد",
            ("Courses", "SetPublished") => "وضعیت انتشار دوره تغییر کرد",
            ("Courses", "SetFree") => "تنظیم رایگان بودن دوره تغییر کرد",
            ("Courses", "Delete") => "دوره حذف شد",
            ("CourseSections", "Create") => "بخش جدیدی به دوره اضافه شد",
            ("CourseLessons", "Create") => "درس جدیدی به دوره اضافه شد",
            ("CourseEpisodes", "UploadEpisode") => "ویدئوی درس بارگذاری شد",
            ("CourseEpisodes", "UpdateEpisode") => "اطلاعات ویدئوی درس تغییر کرد",
            ("CourseEpisodes", "DeleteEpisode") => "ویدئوی درس حذف شد",
            ("Portfolio", "Create") => "نمونه‌کار جدید ثبت شد",
            ("Portfolio", "Update") => "نمونه‌کار تغییر کرد",
            ("Portfolio", "Delete") => "نمونه‌کار حذف شد",
            ("PortfolioImages", "Add") => "تصویر نمونه‌کار اضافه شد",
            ("PortfolioImages", "Delete") => "تصویر نمونه‌کار حذف شد",
            ("PortfolioImages", "Reorder") => "ترتیب تصاویر نمونه‌کار تغییر کرد",
            ("Media", "UploadImage") => "تصویر جدید بارگذاری شد",
            _ => null
        };
    }
}
