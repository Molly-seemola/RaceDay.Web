using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Services;
using System.Text.Json;

namespace RaceDay.Web.Controllers;

public class ParticipantController : Controller
{
    private readonly ApiService _api;

    public ParticipantController(ApiService api)
    {
        _api = api;
    }

    // ==========================================
    // 1. HOME PAGE: Browse Events
    // ==========================================
    public async Task<IActionResult> Index()
    {
        if (HttpContext.Session.GetInt32("UserId") == null)
            return RedirectToAction("Login", "Auth");

        // Get events from the API
        var events = await _api.GetAsync<List<dynamic>>("events");

        // If the API failed or returned null, give it an empty list instead of null
        if (events == null)
        {
            events = new List<dynamic>();
            ViewBag.Error = "Could not load events. Please make sure the API is running.";
        }

        return View(events);
    }

    // ==========================================
    // 2. EVENT DETAILS: Select Category to Enter
    // ==========================================
    public async Task<IActionResult> EventDetails(int id)
    {
        if (HttpContext.Session.GetInt32("UserId") == null)
            return RedirectToAction("Login", "Auth");

        var ev = await _api.GetAsync<dynamic>($"events/{id}");

        if (ev is null)
            return NotFound("This event could not be found.");

        var categories = await _api.GetAsync<List<dynamic>>($"events/{id}/categories");

        ViewBag.Categories = categories;
        return View(ev);
    }

    // ==========================================
    // 3. ENROL: Action to enter an event
    // ==========================================
    [HttpPost]
    public async Task<IActionResult> Enrol(int eventId, int categoryId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToAction("Login", "Auth");

        // Call API to enrol
        var response = await _api.PostAsync($"events/{eventId}/enrol", new { categoryId, paymentStatus = "Paid" });

        if (response.IsSuccessStatusCode)
        {
            TempData["Success"] = "You have successfully enrolled in the event!";
            return RedirectToAction(nameof(MyEvents));
        }

        // If it fails, show the exact API error
        var errorDetails = await response.Content.ReadAsStringAsync();
        TempData["Error"] = $"Could not enrol. API says: {errorDetails}";
        return RedirectToAction(nameof(Index));
    }

    // ==========================================
    // 4. MY EVENTS: View personal enrolments
    // ==========================================
    public async Task<IActionResult> MyEvents()
    {
        // Check if logged in
        if (HttpContext.Session.GetInt32("UserId") == null)
            return RedirectToAction("Login", "Auth");

        // Get enrolments from API
        var enrolments = await _api.GetAsync<List<dynamic>>("enrolments");

        if (enrolments == null)
        {
            enrolments = new List<dynamic>();
            ViewBag.Error = "Could not load your enrolments.";
        }

        return View(enrolments);
    }

    // ==========================================
    // 5. MY RESULTS: View personal race results
    // ==========================================
    public async Task<IActionResult> MyResults()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToAction("Login", "Auth");

        // Ask the API for this participant's results
        // We pass the userId as the participantId (assuming they are linked)
        var results = await _api.GetAsync<List<dynamic>>($"results?participantId={userId}");

        if (results == null)
        {
            results = new List<dynamic>();
            ViewBag.Error = "Could not load results from the API.";
        }

        return View(results);
    }
}