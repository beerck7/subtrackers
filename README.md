# 🚀 SubTrack - Premium WPF Subscription Tracker

**SubTrack** to nowoczesna, wysoce dopracowana wizualnie i funkcjonalnie aplikacja desktopowa w architekturze **MVVM (WPF / .NET 8)** służąca do zarządzania i śledzenia subskrypcji oraz automatycznego podziału kosztów w grupie/rodzinie.

Aplikacja została zaprojektowana z zachowaniem najwyższych standardów UX/UI (ciemny/jasny motyw, animacje mikro-interaktywne, responsywność okna, niestandardowe kontrolki).

---

## 👥 Konta Testowe (Seeded Test Accounts)

Aby ułatwić uruchomienie aplikacji i przetestowanie zaawansowanych funkcji społecznościowych, baza danych SQLite posiada **automatycznie zaimplementowane konta testowe z kompletem danych i płatności historycznych**.

Możesz zalogować się za pomocą poniższych danych:

### 👤 Konto 1
* **Login (Username):** `jan`
* **Hasło (Password):** `jan123`
* *Zawiera:* Aktywne subskrypcje (Netflix Premium, Spotify, Microsoft 365, Prime Video) oraz próbne dane o płatnościach.

### 👤 Konto 2
* **Login (Username):** `anna`
* **Hasło (Password):** `anna123`
* *Zawiera:* Aktywne subskrypcje (Disney+, PS Plus Premium, YouTube Premium) oraz próbne dane o płatnościach.

> [!TIP]
> Zaloguj się na jedno konto, wyślij zaproszenie do drugiego w zakładce **Rodzina**, następnie zaloguj się na drugie konto i zaakceptuj zaproszenie, aby przetestować pełen dwukierunkowy system społecznościowego współdzielenia kosztów w czasie rzeczywistym!

---

## ✨ Kluczowe Funkcjonalności

1. **Pulpit nawigacyjny (Dashboard)**:
   * Podsumowanie miesięcznych i rocznych kosztów.
   * Najbliższe nadchodzące opłaty z odliczaniem dni.
   * Rozkład wydatków na kategorie (wykres pierścieniowy).
   * Historia ostatnich płatności.

2. **Menedżer Subskrypcji**:
   * Pełny CRUD subskrypcji z cyklem rozliczeniowym (miesięczny/roczny).
   * Status aktywności (aktywna/wstrzymana) z możliwością szybkiego przełączania.
   * Logowanie i rejestrowanie faktycznie opłaconych cykli jednym kliknięciem.

3. **System Społecznościowy i Rodzina (SubTrack Social)**:
   * **Wyszukiwarka użytkowników**: szukanie po nazwie lub e-mailu.
   * **Dwuetapowe zapraszanie**: system wysyłania, odbierania i akceptowania zaproszeń do rodziny.
   * **Zarządzanie powiązaniami**: wyświetlanie połączonych kont, ich udziału kosztowego oraz liczby współdzielonych usług.
   * **Wygodne zarządzanie z poziomu karty subskrypcji**: bezpośredni odnośnik do zarządzania i podziału kosztów z poziomu widoku szczegółów usługi.

4. **Elastyczny Podział Kosztów**:
   * Podział ceny subskrypcji na dowolną liczbę zaakceptowanych członków rodziny.
   * Automatyczne obliczanie i prezentacja stawki jednostkowej oraz pełnej kwoty.
   * Funkcja rezygnacji (opt-out) – możliwość bezpiecznego opuszczenia współdzielonej subskrypcji przez zaproszonego członka rodziny bez usuwania jej dla reszty grupy.

5. **Wygodne Ustawienia**:
   * Dynamiczny przełącznik motywu (Jasny/Ciemny) działający bez restartu aplikacji.
   * Pełne zapisywanie konfiguracji użytkownika.

---

## ⚙️ Wymagania i Uruchomienie

### Wymagania:
* SDK **.NET 8.0** lub nowsze.
* System Windows (ze względu na technologię WPF).

### Uruchomienie:
1. Sklonuj repozytorium.
2. Otwórz terminal w katalogu głównym projektu (`SubscriptionTracker`).
3. Wykonaj polecenie:
   ```bash
   dotnet run
   ```
4. Baza danych SQLite (`subscriptions.db`) zostanie automatycznie utworzona i zasilona przy pierwszym uruchomieniu.
