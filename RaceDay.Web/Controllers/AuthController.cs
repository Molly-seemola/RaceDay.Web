using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Services;
using System.Text.Json;

namespace RaceDay.Web.Controllers;

public class AuthController : Controller
{
    private readonly ApiService _api;
    
    public AuthController(ApiService api)
    {
        _api = api;
    }

    // ==========================================
    // LOGIN
    // ==========================================
    [HttpGet]
    public IActionResult Login()
    {
        // If already logged in, redirect them to their dashboard
        if (HttpContext.Session.GetInt32("UserId") != null)
        {
            return HttpContext.Session.GetString("UserRole") == "Organiser" 
                ? RedirectToAction("Dashboard", "Organiser") 
                : RedirectToAction("Index", "Participant");
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ViewBag.Error = "Please enter both email and password.";
            return View();
        }

        try
        {
            // 1. Call the API
            var response = await _api.PostAsync("auth/login", new { email, password });

            // 2. Check if successful
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                
                try 
                {
                    // Parse the user data returned by the API
                    var user = JsonDocument.Parse(json).RootElement.GetProperty("user");
                    
                    // Save to MVC Session
                    HttpContext.Session.SetInt32("UserId", user.GetProperty("userId").GetInt32());
                    HttpContext.Session.SetString("UserRole", user.GetProperty("role").GetString()!);
                    HttpContext.Session.SetString("UserName", user.GetProperty("firstName").GetString()!);

                    // Redirect based on role
                    if (user.GetProperty("role").GetString() == "Organiser")
                        return RedirectToAction("Dashboard", "Organiser");
                    else
                        return RedirectToAction("Index", "Participant");
                }
                catch (Exception ex)
                {
                    ViewBag.Error = $"Error parsing API response: {ex.Message}";
                    return View();
                }
            }

            // If login fails, show the API error
            var errorDetails = await response.Content.ReadAsStringAsync();
            ViewBag.Error = $"Login Failed. API says: {(int)response.StatusCode} - {errorDetails}";
            return View();
        }
        catch (Exception ex)
        {
            // 🚨 THIS CATCHES THE CONNECTION ERROR
            ViewBag.Error = $"Could not connect to the API. Is it running? Error: {ex.Message}";
            return View();
        }
    }

    // ==========================================
    // REGISTER
    // ==========================================
    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(string email, string password, string firstName, string lastName, string role)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(role))
        {
            ViewBag.Error = "Please fill in all required fields.";
            return View();
        }

        try
        {
            // Build the payload expected by the Part 2 API RegisterDto
            var registerData = new
            {
                email = email,
                password = password,
                firstName = firstName,
                lastName = lastName,
                role = role,
                // Defaults for required fields in the API depending on role
                dateOfBirth = role == "Participant" ? DateTime.Now.AddYears(-20).ToString("yyyy-MM-dd") : null,
                gender = role == "Participant" ? "Other" : null,
                emergencyContact = role == "Participant" ? "None" : null,
                emergencyPhone = role == "Participant" ? "0000000000" : null,
                companyName = role == "Organiser" ? "My Events" : null,
                contactNumber = role == "Organiser" ? "0000000000" : null
            };

            // Call the API
            var response = await _api.PostAsync("auth/register", registerData);

            if (response.IsSuccessStatusCode)
            {
                // Registration successful, redirect to Login
                TempData["Success"] = "Registration successful! Please log in.";
                return RedirectToAction(nameof(Login));
            }

            // Show the exact error if it fails
            var errorDetails = await response.Content.ReadAsStringAsync();
            ViewBag.Error = $"Registration Failed. API says: {(int)response.StatusCode} - {errorDetails}";
            return View();
        }
        catch (Exception ex)
        {
            ViewBag.Error = $"Could not connect to the API. Is it running? Error: {ex.Message}";
            return View();
        }
    }

    // ==========================================
    // LOGOUT
    // ==========================================
    public async Task<IActionResult> Logout()
    {
        try
        {
            // Tell the API to clear the session
            await _api.PostAsync("Auth/logout", new { });
        }
        catch 
        {
            // Ignore errors if the API is down during logout
        }
        
        // Clear the MVC session
        HttpContext.Session.Clear();
        
        // Redirect to Home
        return RedirectToAction("Index", "Home");
    }
}