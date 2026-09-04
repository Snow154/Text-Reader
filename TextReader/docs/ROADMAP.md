# TextReader – roadmapa

Roadmapa je zoradená podľa rizika a používateľskej hodnoty. Jednotlivé fázy majú byť malé, samostatne overiteľné a ukončené funkčným buildom.

## P0 – Stabilný základ

### Cieľ

Odstrániť technické chyby, ktoré komplikujú ďalší vývoj.

### Úlohy

- opraviť nullable warningy,
- zaviesť async command namiesto `async void` command handlerov,
- pridať cancellation a ochranu pred zastaranými výsledkami načítania,
- bezpečne zrušiť indexovanie, search a načítanie pri otvorení iného súboru,
- zaviesť jednotné hlásenie chýb,
- pridať samostatný testovací projekt.

### Výstup

- build bez warningov,
- základné unit testy pre index a získanie riadkov,
- UI zostáva responzívne pri rýchlej navigácii.

## P1 – Korektný textový viewport

### Cieľ

Opraviť navigáciu a spraviť zobrazenie textu intuitívne.

### Úlohy

- vytvoriť model `VisibleLine` s číslom a textom riadku,
- zobraziť synchronizovaný gutter s číslami riadkov,
- zjednotiť veľkosť viewportu a buffer načítaných riadkov,
- vypočítať počet viditeľných riadkov podľa veľkosti okna,
- zabezpečiť korektný prvý a posledný riadok,
- vypnúť word wrap ako predvolené správanie,
- pridať prepínač word wrap,
- pridať výber a kopírovanie textu,
- zobraziť aktuálny rozsah riadkov v status bare.

### Akceptácia

- `End` zobrazí skutočný posledný riadok,
- `Home` zobrazí prvý riadok,
- resize okna nepoškodí navigáciu,
- čísla riadkov zostanú synchronizované s obsahom,
- používateľ dokáže označiť a skopírovať text.

## P2 – Vyhľadávanie v1

### Cieľ

Dodať predvídateľné vyhľadávanie vhodné pre veľké súbory.

### Úlohy

- presunúť search logiku do `ISearchService` a `SearchViewModel`,
- reprezentovať výsledok riadkom, stĺpcom a dĺžkou,
- nájsť všetky výskyty vrátane viacerých výskytov na rovnakom riadku,
- zvýrazniť aktívny výsledok,
- pridať `Next`, `Previous` a wrap-around navigáciu,
- pridať progress podľa prečítaných dát,
- pridať `Cancel`,
- zobraziť `aktuálny / celkový počet výsledkov`,
- pridať `Match case` a `Whole word`.

### Akceptácia

- UI počas hľadania nezamrzne,
- nové hľadanie zruší predchádzajúce,
- každý výskyt sa dá navštíviť presne raz,
- výsledok je viditeľne zvýraznený,
- prázdny alebo nenájdený výraz má zrozumiteľný stav.

## P3 – Refaktoring architektúry

### Cieľ

Oddeliť UI stav od práce so súbormi a externými zdrojmi.

### Úlohy

- rozdeliť `MainViewModel` na document, navigation a search časť,
- extrahovať file dialogs, download, temp files a save do služieb,
- zaviesť dependency injection,
- odstrániť priame použitie `MessageBox` a `Application.Current.Dispatcher` z doménovej logiky,
- definovať konzistentné stavy `Idle`, `Loading`, `Indexing`, `Searching`, `Error`,
- doplniť unit testy ViewModelov so stub/fake službami.

### Akceptácia

- jednotlivé služby sa dajú testovať bez WPF okna,
- `MainViewModel` iba koordinuje obrazovku,
- žiadny ViewModel priamo nevytvára `HttpClient`, stream alebo dialóg.

## P4 – Robustná práca so súbormi

### Cieľ

Podporiť reálne textové súbory a hraničné prípady.

### Úlohy

- korektne podporiť UTF-8, UTF-8 BOM, UTF-16 LE/BE a ASCII,
- zobraziť detegovaný encoding,
- ošetriť súbor bez posledného newline,
- ošetriť prázdny súbor a extrémne dlhý riadok,
- reagovať na zmenu, premenovanie alebo odstránenie súboru,
- používať bezpečné systémové temp súbory,
- doplniť timeout, validáciu URL, progress a size limit downloadu.

### Akceptácia

- index a zobrazené riadky sa zhodujú pre podporované encodingy,
- aplikácia nespadne pri prázdnom alebo zmenenom súbore,
- dočasné súbory sa odstránia aj po neúspešnej operácii.

## P5 – Produktové rozšírenia

Implementovať podľa potreby až po stabilizácii jadra:

- `Go to line` (`Ctrl+G`),
- drag & drop,
- recent files,
- tail/follow mode pre rastúce logy,
- `Find all` panel a export výsledkov,
- light/dark téma,
- nastavenie fontu a veľkosti,
- regex search,
- otvorenie viacerých súborov v taboch.

## Navrhované míľniky

| Míľnik | Obsah | Výsledok |
|---|---|---|
| M1 | P0 + P1 | Spoľahlivý viewer s číslami riadkov a kopírovaním |
| M2 | P2 | Kompletné a zrušiteľné vyhľadávanie |
| M3 | P3 + P4 | Testovateľná architektúra a robustná práca so súbormi |
| M4 | vybrané P5 | Portfóliovo zaujímavé produktové funkcie |

## Odporúčaný prvý implementačný krok

Začať malým vertikálnym rezom:

1. pridať testovací projekt,
2. otestovať posledný riadok a rôzne zakončenia súborov,
3. zaviesť `VisibleLine`,
4. prerobiť viewport na kolekciu viditeľných riadkov,
5. pridať line-number gutter,
6. overiť scroll na začiatok a koniec.

Tento krok rieši najviditeľnejší bug a zároveň pripraví UI model potrebný na zvýrazňovanie výsledkov.

