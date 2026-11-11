using System;

namespace Specter.Services;

public enum AppLanguage { English, Hebrew }

public static class AppStrings
{
    public static AppLanguage Language { get; private set; } = AppLanguage.English;
    public static event Action? LanguageChanged;

    public static void Toggle()
    {
        Language = Language == AppLanguage.English ? AppLanguage.Hebrew : AppLanguage.English;
        LanguageChanged?.Invoke();
    }

    // ── HomePage ──────────────────────────────────────────────────────────
    public static string HomeTitle       => Language == AppLanguage.English ? "Home"              : "בית";
    public static string Welcome(string user) => Language == AppLanguage.English
        ? $"Welcome, {user}" : $"ברוך הבא, {user}";
    public static string ComingSoon      => Language == AppLanguage.English ? "Sales tools coming soon" : "כלי מכירות בקרוב";
    public static string Logout          => Language == AppLanguage.English ? "Logout"            : "התנתק";
    public static string ClientsBtn      => Language == AppLanguage.English ? "＋ Clients"        : "＋ לקוחות";
    public static string CalendarBtn     => Language == AppLanguage.English ? " Calendar"         : " לוח שנה";
    public static string ToggleLang      => Language == AppLanguage.English ? " עברית"          : " English";

    // ── ClientsPage ───────────────────────────────────────────────────────
    public static string ClientsTitle    => Language == AppLanguage.English ? "Clients"           : "לקוחות";
    public static string SearchPlaceholder => Language == AppLanguage.English ? "Search..."       : "חפש...";
    public static string AddBtn          => Language == AppLanguage.English ? "＋ Add"            : "＋ הוסף";
    public static string NoClients       => Language == AppLanguage.English ? "No clients yet."   : "אין לקוחות עדיין.";
    public static string CallAttempts(int n) => Language == AppLanguage.English
        ? $"Call attempts: {n}" : $"ניסיונות שיחה: {n}";
    public static string NamePlaceholder    => Language == AppLanguage.English ? "Name"           : "שם";
    public static string PhonePlaceholder   => Language == AppLanguage.English ? "Phone number"   : "מספר טלפון";
    public static string ClientNumPlaceholder => Language == AppLanguage.English ? "Client number" : "מספר לקוח";
    public static string NotesPlaceholder   => Language == AppLanguage.English ? "Notes"          : "הערות";
    public static string ScheduledDate      => Language == AppLanguage.English ? "Scheduled Date" : "תאריך מתוכנן";
    public static string AddClientTitle     => Language == AppLanguage.English ? "Add Client"     : "הוסף לקוח";
    public static string EditClientTitle    => Language == AppLanguage.English ? "Edit Client"    : "ערוך לקוח";
    public static string Save               => Language == AppLanguage.English ? "Save"           : "שמור";
    public static string LogCallBtn         => Language == AppLanguage.English ? "Log call attempt" : "רשום ניסיון שיחה";
    public static string LogCallTitle       => Language == AppLanguage.English ? "Log call attempt" : "רשום ניסיון שיחה";
    public static string LogCallMsg         => Language == AppLanguage.English
        ? "Record one call attempt? Include unanswered calls, busy lines, voicemail, and answered calls."
        : "לרשום ניסיון שיחה? כולל שיחות ללא מענה, קו תפוס, תא קולי ושיחות שנענו.";
    public static string LogAttempt         => Language == AppLanguage.English ? "Log attempt"   : "רשום";
    public static string Cancel             => Language == AppLanguage.English ? "Cancel"         : "ביטול";
    public static string CallLogged         => Language == AppLanguage.English ? "Call attempt logged" : "ניסיון שיחה נרשם";
    public static string Ok                 => Language == AppLanguage.English ? "OK"             : "אישור";
    public static string Error              => Language == AppLanguage.English ? "Error"          : "שגיאה";
    public static string DeleteClient       => Language == AppLanguage.English ? "Delete Client"  : "מחק לקוח";
    public static string Delete             => Language == AppLanguage.English ? "Delete"         : "מחק";
    public static string DeleteMsg(string name) => Language == AppLanguage.English
        ? $"Delete {name}?" : $"למחוק את {name}?";
    public static string NameRequired       => Language == AppLanguage.English ? "Name is required." : "שם הוא שדה חובה.";
    public static string Mismatch           => Language == AppLanguage.English ? "Mismatch"       : "אי התאמה";
    public static string ConfirmPhone       => Language == AppLanguage.English ? "Confirm Phone Number" : "אמת מספר טלפון";
    public static string ConfirmPhoneMsg    => Language == AppLanguage.English
        ? "Re-enter the phone number to verify:" : "הזן שוב את מספר הטלפון לאימות:";
    public static string PhoneMismatchMsg   => Language == AppLanguage.English
        ? "Phone numbers do not match. Please try again." : "מספרי הטלפון אינם תואמים. נסה שוב.";
    public static string PhoneLengthError   => Language == AppLanguage.English
        ? "Phone number must be exactly 10 digits." : "מספר הטלפון חייב להיות בדיוק 10 ספרות.";
    public static string ConfirmClientNum   => Language == AppLanguage.English ? "Confirm Client Number" : "אמת מספר לקוח";
    public static string ConfirmClientNumMsg => Language == AppLanguage.English
        ? "Re-enter the client number to verify:" : "הזן שוב את מספר הלקוח לאימות:";
    public static string ClientNumMismatchMsg => Language == AppLanguage.English
        ? "Client numbers do not match. Please try again." : "מספרי הלקוח אינם תואמים. נסה שוב.";
    public static string ClientNumLengthError => Language == AppLanguage.English
        ? "Client number must be exactly 10 digits." : "מספר הלקוח חייב להיות בדיוק 10 ספרות.";
}
