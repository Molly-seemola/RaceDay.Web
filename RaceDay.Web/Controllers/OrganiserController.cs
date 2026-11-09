using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Services;
using System.Text.Json;

namespace RaceDay.Web.Controllers;

public class OrganiserController : Controller
{
    private readonly ApiService _api;

    public OrganiserController(ApiService api)
    {
        _api = api;
    }

    // Helper to check if the logged-in user is an Organiser
    private bool IsOrganiser() => HttpContext.Session.GetString("UserRole") == "Organiser";

    // ==========================================
    // 1. DASHBOARD
    // ==========================================
    public async Task<IActionResult> Dashboard()
    {
        if (!IsOrganiser()) return RedirectToAction("Login", "Auth");

        // Get all events from the API
        var events = await _api.GetAsync<List<dynamic>>("events");

        if (events == null)
        {
            events = new List<dynamic>();
            ViewBag.Error = "Could not load events from API.";
        }

        return View(events);
    }

    // ==========================================
    // 2. SHOW CREATE EVENT FORM
    // ==========================================
    [HttpGet]
    public IActionResult CreateEvent()
    {
        if (!IsOrganiser()) return RedirectToAction("Login", "Auth");
        return View();
    }

    // ==========================================
    // 3. HANDLE CREATE EVENT SUBMISSION
    // ==========================================
    [HttpPost]
    public async Task<IActionResult> CreateEvent(string name, string description, DateTime eventDate, string location, decimal distance, string eventType, IFormFile image)
    {
        if (!IsOrganiser()) return RedirectToAction("Login", "Auth");

        string? imageUrl = null;

        // If an image was uploaded, send it to the API (which will send it to Azure)
        if (image != null && image.Length > 0)
        {
            imageUrl = await _api.UploadImageAsync(image);
        }

        // Build the data to send to the API
        var eventData = new
        {
            name = name,
            description = description,
            eventDate = eventDate.ToString("yyyy-MM-dd"),
            startTime = "06:00:00",
            location = location,
            distance = distance,
            eventType = eventType,
            status = "Published",
            maxParticipants = 1000,
            imageUrl = imageUrl
        };

        // Call the API
        var response = await _api.PostAsync("events", eventData);

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction(nameof(Dashboard));
        }

        // Show the exact error from the API
        var errorDetails = await response.Content.ReadAsStringAsync();
        ViewBag.Error = $"API says: {(int)response.StatusCode} - {errorDetails}";

        return View();
    }

    // ==========================================
    // 4. VIEW ENROLMENTS FOR AN EVENT
    // ==========================================
    public async Task<IActionResult> ViewEnrolments(int id)
    {
        if (!IsOrganiser()) return RedirectToAction("Login", "Auth");

        // Get the event details
        var ev = await _api.GetAsync<dynamic>($"events/{id}");
        if (ev is null) return NotFound("Event not found");

        // Ask the API for enrolments. We pass the eventId to filter them.
        var eventEnrolments = await _api.GetAsync<List<dynamic>>($"enrolments?eventId={id}");

        if (eventEnrolments == null)
        {
            eventEnrolments = new List<dynamic>();
            ViewBag.Error = "Could not load enrolments from the API. The API might be down or you don't have permission.";
        }

        ViewBag.EventName = ev.GetProperty("name").GetString();
        ViewBag.EventId = id;

        return View(eventEnrolments);
    }

    // ==========================================
    // 5. SUBMIT A RESULT
    // ==========================================
    [HttpPost]
    public async Task<IActionResult> SubmitResult(int enrolmentId, string finishTime, int position, int eventId)
    {
        if (!IsOrganiser()) return RedirectToAction("Login", "Auth");

        // Make sure finishTime is properly formatted (e.g., "02:30:00")
        // If the user typed "2:30", we format it to "02:30:00"
        if (TimeSpan.TryParse(finishTime, out var parsedTime))
        {
            finishTime = parsedTime.ToString(@"hh\:mm\:ss");
        }

        // Send result to API
        var response = await _api.PostAsync("results", new
        {
            enrolmentId = enrolmentId,
            finishTime = finishTime,
            position = position,
            status = "Confirmed"
        });

        if (response.IsSuccessStatusCode)
        {
            TempData["Success"] = "Result saved successfully!";
        }
        else
        {
            // 🚨 SHOW THE EXACT ERROR FROM THE API
            var errorDetails = await response.Content.ReadAsStringAsync();
            TempData["Error"] = $"API says: {(int)response.StatusCode} - {errorDetails}";
        }

        // Go back to the enrolments page
        return RedirectToAction(nameof(ViewEnrolments), new { id = eventId });
    }
}