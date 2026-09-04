# TextReader – feature specification a požiadavky

## 1. Rozsah produktu

TextReader je read-only WPF aplikácia na prehliadanie a vyhľadávanie vo veľkých textových súboroch bez načítania celého obsahu do pamäte.

### Primárne podporované zdroje

- lokálny textový súbor,
- textový obsah dostupný cez HTTP/HTTPS,
- generovaný veľký testovací súbor.

### Podporované typy obsahu

Typ súboru nie je funkčne obmedzený príponou. Dialóg má zvýrazniť najmä `.txt`, `.log`, `.md`, `.csv`, `.json`, `.xml` a `.html`.

## 2. Používateľské scenáre

### US-01: Otvorenie veľkého súboru

Ako používateľ chcem otvoriť veľký textový súbor a ihneď vidieť jeho začiatok, aby som nemusel čakať na načítanie celého súboru.

Akceptačné kritériá:

- prvý náhľad sa zobrazí pred dokončením indexovania,
- UI počas indexovania zostáva responzívne,
- používateľ vidí stav a progres indexovania,
- neúspech obsahuje zrozumiteľnú chybu a možnosť pokračovať.

### US-02: Navigácia v súbore

Ako používateľ chcem scrollovať, používať klávesnicu a prejsť na konkrétny riadok.

Akceptačné kritériá:

- fungujú koliesko myši, scrollbar, šípky, Page Up/Down, Home a End,
- scrollbar reprezentuje celý rozsah fyzických riadkov,
- prvý a posledný riadok sú vždy dostupné,
- status bar zobrazuje viditeľný rozsah a celkový počet riadkov,
- `Ctrl+G` otvorí dialóg na prechod na riadok.

### US-03: Čísla riadkov

Ako používateľ chcem vidieť číslo každého fyzického riadku.

Akceptačné kritériá:

- číslovanie začína od 1,
- gutter zostáva zarovnaný s textom,
- zalomený text nevytvorí nové číslo fyzického riadku,
- šírka guttera sa prispôsobí celkovému počtu riadkov.

### US-04: Výber a kopírovanie

Ako používateľ chcem označiť text a skopírovať ho do clipboardu.

Akceptačné kritériá:

- výber funguje myšou aj klávesnicou,
- `Ctrl+C` kopíruje označený text,
- kontextové menu obsahuje `Copy` a `Select all`,
- UI jasne definuje, či `Select all` platí pre viewport alebo celý súbor,
- čísla riadkov sa štandardne nekopírujú spolu s textom.

### US-05: Vyhľadanie výrazu

Ako používateľ chcem nájsť všetky výskyty výrazu a navigovať medzi nimi.

Akceptačné kritériá:

- `Ctrl+F` otvorí search panel a nastaví focus do inputu,
- `Enter` alebo `F3` prejde na ďalší výsledok,
- `Shift+Enter` alebo `Shift+F3` prejde na predchádzajúci,
- navigácia pokračuje cez viac výskytov na rovnakom riadku,
- po dosiahnutí konca môže pokračovať od začiatku a oznámi wrap-around,
- aktívny výskyt je vizuálne zvýraznený,
- panel zobrazuje pozíciu vo formáte `N / total`,
- používateľ môže zapnúť `Match case` a `Whole word`.

### US-06: Dlhé vyhľadávanie

Ako používateľ chcem vedieť, že hľadanie pokračuje, a mať možnosť ho zrušiť.

Akceptačné kritériá:

- počas úplného skenovania sa zobrazuje percentuálny alebo determinovaný progress,
- hľadanie sa dá zrušiť,
- nová query zruší predchádzajúcu operáciu,
- zrušenie sa nepovažuje za chybu,
- zastaraný výsledok nesmie prepísať aktuálny stav UI.

### US-07: Otvorenie URL

Ako používateľ chcem načítať textový obsah z URL.

Akceptačné kritériá:

- podporované sú iba HTTP a HTTPS URL,
- download zobrazuje progress, ak server poskytuje veľkosť,
- operácia má timeout a dá sa zrušiť,
- HTTP chyba sa zobrazí zrozumiteľne,
- je definovaný konfigurovateľný maximálny objem downloadu,
- dočasný súbor sa bezpečne odstráni.

## 3. Funkčné požiadavky

### FR-01: Indexovanie

- Aplikácia musí vytvoriť index umožňujúci efektívny prístup k ľubovoľnému riadku.
- Index musí korektne pracovať s podporovanými encodingmi a newline formátmi `LF`, `CRLF` a `CR`.
- Posledný riadok bez newline musí byť započítaný.
- Indexovanie musí podporovať progress a cancellation.

### FR-02: Stránkovanie a viewport

- Aplikácia nesmie držať celý veľký súbor ako jeden UI string.
- Viewport musí obsahovať iba viditeľné riadky a primeraný buffer.
- Každý riadok musí mať aspoň `LineNumber` a `Text`.
- Každá požiadavka na načítanie musí byť identifikovateľná alebo zrušiteľná, aby staré výsledky neprepísali nové.

### FR-03: Search model

Výsledok vyhľadávania musí obsahovať minimálne:

```csharp
public sealed record SearchMatch(
    long LineNumber,
    int StartColumn,
    int Length);
```

- Search nesmie skončiť po prvom výskyte na riadku.
- Porovnanie musí podporovať case-sensitive a case-insensitive režim.
- Whole-word režim musí mať explicitne definované hranice slova.

### FR-04: Zvýraznenie

- Viditeľné riadky musia poznať relevantné match ranges.
- Aktívny match musí používať inú farbu než ostatné viditeľné matches.
- Zvýraznenie nesmie meniť pôvodný text ani jeho stĺpcové indexy.

### FR-05: Stav aplikácie

Aplikácia musí rozlišovať aspoň:

- `Idle`,
- `LoadingPreview`,
- `Indexing`,
- `Ready`,
- `Searching`,
- `Downloading`,
- `Error`.

Stav musí určovať dostupnosť príkazov a obsah status baru.

## 4. Nefunkčné požiadavky

### NFR-01: Výkon

- UI thread nesmie vykonávať dlhé diskové ani sieťové operácie.
- Scroll a základná navigácia musia pôsobiť plynulo aj počas background operácie.
- Spotreba pamäte nemá rásť lineárne s veľkosťou otvoreného súboru.
- Výkonnostné testy majú používať minimálne súbor s 1 000 000 riadkami.

Konkrétne časové limity je vhodné stanoviť po vytvorení benchmarku na referenčnom zariadení.

### NFR-02: Spoľahlivosť

- Aplikácia nesmie spadnúť pri poškodenom, prázdnom, zamknutom alebo počas čítania odstránenom súbore.
- Výnimky z background operácií musia byť zachytené a zalogované.
- Otvorenie nového dokumentu musí ukončiť operácie patriace starému dokumentu.

### NFR-03: Testovateľnosť

- File access, search, download, dialógy a temp storage musia byť dostupné cez rozhrania.
- ViewModel testy nesmú vyžadovať otvorenie WPF okna.
- Kritické scenáre musia byť pokryté automatizovanými testami.

### NFR-04: Prístupnosť a UX

- Všetky hlavné operácie musia byť dostupné klávesnicou.
- Focus musí byť vizuálne rozpoznateľný.
- Stav nemá byť komunikovaný iba farbou.
- Chybové správy majú používateľovi povedať, čo sa stalo a čo môže urobiť.

## 5. Testovacia matica

### Súbory

- prázdny súbor,
- jeden riadok bez newline,
- jeden riadok s newline,
- `LF`, `CRLF` a `CR`,
- UTF-8 bez BOM a s BOM,
- UTF-16 LE a BE,
- veľmi dlhý riadok,
- 1 000 000+ riadkov,
- súbor zmenený alebo odstránený počas čítania.

### Vyhľadávanie

- žiadny výskyt,
- jeden výskyt,
- viac výskytov na jednom riadku,
- výsledok na prvom a poslednom riadku,
- query cez rozdielne veľkosti písmen,
- whole-word hranice,
- zrušenie počas operácie,
- zmena query počas operácie,
- wrap-around dopredu aj dozadu.

### Navigácia

- Home a End,
- Page Up a Page Down,
- scrollbar na minimum a maximum,
- resize okna,
- word wrap zapnutý a vypnutý,
- rýchle opakované scrollovanie.

## 6. Mimo rozsahu prvej verzie

- editovanie a ukladanie zmien v dokumente,
- syntax highlighting podľa programovacieho jazyka,
- spolupráca viacerých používateľov,
- binárny alebo hex viewer,
- porovnávanie dvoch súborov.

Tieto funkcie sa nemajú implementovať, kým nie sú splnené míľniky M1 až M3 z roadmapy.

## 7. Definition of Done

Feature je dokončená iba vtedy, keď:

- spĺňa svoje akceptačné kritériá,
- má primerané automatizované testy,
- build nemá nové warningy,
- background operácie podporujú chyby a cancellation,
- používateľský stav je viditeľný v UI,
- existujúce kritické scenáre zostávajú funkčné.

