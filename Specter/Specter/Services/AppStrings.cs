using System;

namespace Specter.Services;

public enum AppLanguage { English, Hebrew }

public static class AppStrings
{
    public static AppLanguage Language { get; private set; } = AppLanguage.English;
    public static bool PrivacyMode { get; private set; } = false;
    public static event Action? LanguageChanged;

    public static void Toggle()
    {
        Language = Language == AppLanguage.English ? AppLanguage.Hebrew : AppLanguage.English;
        LanguageChanged?.Invoke();
    }

    public static void TogglePrivacy()
    {
        PrivacyMode = !PrivacyMode;
        LanguageChanged?.Invoke();
    }

    public static string Mask(string? value) =>
        string.IsNullOrEmpty(value) ? "" : new string('*', Math.Min(value.Length, 8));

    // ── HomePage ──────────────────────────────────────────────────────────
    public static string HomeTitle       => Language == AppLanguage.English ? "Home"              : "בית";
    public static string Welcome(string user) => Language == AppLanguage.English
        ? $"Welcome, {user}" : $"ברוך הבא, {user}";
    public static string ComingSoon      => Language == AppLanguage.English ? "Sales tools coming soon" : "כלי מכירות בקרוב";
    public static string Logout          => Language == AppLanguage.English ? "Logout"            : "התנתק";
    public static string ClientsBtn      => Language == AppLanguage.English ? "＋ Clients"        : "＋ לקוחות";
    public static string CalendarBtn     => Language == AppLanguage.English ? " Calendar"         : " לוח שנה";
    public static string ArchivedBtn     => Language == AppLanguage.English ? " Archived"         : " ארכיון";
    public static string ToggleLang      => Language == AppLanguage.English ? " עברית"          : " English";
    public static string PrivacyOn       => Language == AppLanguage.English ? " Privacy ON"       : " פרטיות פועל";
    public static string PrivacyOff      => Language == AppLanguage.English ? " Privacy OFF"      : " פרטיות כבוי";

    // ── ClientsPage ───────────────────────────────────────────────────────
    public static string ClientsTitle    => Language == AppLanguage.English ? "Clients"           : "לקוחות";
    public static string SearchPlaceholder => Language == AppLanguage.English ? "Search..."       : "חפש...";
    public static string AddBtn          => Language == AppLanguage.English ? "＋ Add"            : "＋ הוסף";
    public static string NoClients       => Language == AppLanguage.English ? "No clients yet."   : "אין לקוחות עדיין.";
    public static string Archive         => Language == AppLanguage.English ? "Archive"           : "ארכיון";
    public static string ArchivedClients => Language == AppLanguage.English ? "Archived Clients"  : "לקוחות בארכיון";
    public static string ArchivedOn      => Language == AppLanguage.English ? "Archived:"         : "בוארכב:";
    public static string CallAttempts(int n) => Language == AppLanguage.English
        ? $"Call attempts: {n}" : $"ניסיונות שיחה: {n}";
    public static string TitlePlaceholder   => Language == AppLanguage.English ? "Title"          : "כותרת";
    public static string NamePlaceholder    => Language == AppLanguage.English ? "Name"           : "שם";
    public static string PhonePlaceholder   => Language == AppLanguage.English ? "Phone number"   : "מספר טלפון";
    public static string ClientNumPlaceholder => Language == AppLanguage.English ? "Client number" : "מספר לקוח";
    public static string NotesPlaceholder   => Language == AppLanguage.English ? "Notes"          : "הערות";
    public static string CopyTitleToNotes   => Language == AppLanguage.English ? "↓ Copy Title to Notes" : "↓ העתק כותרת להערות";
    public static string CategoryLabel      => Language == AppLanguage.English ? "Category"            : "קטגוריה";
    public static string NoCategoryOption   => Language == AppLanguage.English ? "— No Category —"     : "— ללא קטגוריה —";
    public static string AddCategory        => Language == AppLanguage.English ? "+ Add Category"       : "+ הוסף קטגוריה";
    public static string CategoryName       => Language == AppLanguage.English ? "Category name"        : "שם קטגוריה";
    public static string ScheduledDate      => Language == AppLanguage.English ? "Scheduled Date" : "תאריך מתוכנן";
    public static string ClosingChanceLabel => Language == AppLanguage.English ? "Closing chance (0-100%)" : "סיכוי סגירה (0-100%)";
    public static string ClosingChanceError => Language == AppLanguage.English ? "Closing chance must be a number between 0 and 100." : "סיכוי הסגירה חייב להיות מספר בין 0 ל-100.";
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
    public static string NotesRequired      => Language == AppLanguage.English ? "Notes are required." : "הערות הן שדה חובה.";
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
