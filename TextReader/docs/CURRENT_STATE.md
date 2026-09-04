# TextReader – zhodnotenie súčasného stavu

## Účel dokumentu

Tento dokument sumarizuje pripomienky z technického interview a porovnáva ich so súčasnou implementáciou aplikácie. Slúži ako východiskový bod pre roadmapu a produktovú špecifikáciu.

## Produktová vízia

TextReader má byť rýchly desktopový prehliadač veľkých textových súborov, logov a podobných dát, ktoré nie je vhodné načítať celé do pamäte. Hlavnou hodnotou aplikácie má byť:

- korektná navigácia aj vo veľmi veľkých súboroch,
- čitateľné a intuitívne používateľské rozhranie,
- rýchle, zrušiteľné a vizuálne zrozumiteľné vyhľadávanie,
- stabilná odozva UI počas indexovania a vyhľadávania.

## Zhrnutie spätnej väzby

### 1. Neintuitívne zobrazenie textu

Spätná väzba:

- chýbajú čísla riadkov,
- text sa nedá štandardne označiť a kopírovať,
- pri scrollovaní na koniec nemusí byť jasne zobrazený skutočný koniec súboru.

Súčasný stav:

- text sa zobrazuje cez `TextBlock`, ktorý neposkytuje štandardný výber textu,
- čísla riadkov nie sú súčasťou UI,
- načítava sa 1 000 riadkov, ale maximálna pozícia scrollbaru počíta s viewportom 100 riadkov,
- skutočný počet viditeľných riadkov sa nepočíta podľa veľkosti okna,
- zapnuté zalamovanie textu spôsobuje rozdiel medzi fyzickým a vizuálnym riadkom.

Riziká:

- používateľ nevie jednoznačne určiť svoju pozíciu v súbore,
- scrollbar nereprezentuje presne obsah na obrazovke,
- rýchle scrollovanie môže spustiť viac súbežných načítaní a starší výsledok môže prepísať novší.

### 2. Vyhľadávanie

Spätná väzba:

- na veľkých súboroch je pomalé,
- nemá indikáciu progresu,
- hľadaný text nie je zvýraznený,
- nie je viditeľný počet výskytov.

Súčasný stav:

- ViewModel obsahuje hodnoty pre zvýraznenie, ale XAML ich nepoužíva,
- `IndexOf` nájde iba prvý výskyt na riadku,
- ďalšie hľadanie pokračuje nasledujúcim riadkom a môže preskočiť viac výskytov na rovnakom riadku,
- nie je dostupné `Cancel`, percentuálny progres ani počet výsledkov,
- súbežné požiadavky nie sú koordinované pomocou `CancellationToken`.

### 3. Architektúra

Spätná väzba:

- `MainViewModel` má príliš veľa zodpovedností,
- kód nie je dostatočne rozdelený.

Súčasný `MainViewModel` rieši:

- otvorenie súboru a dialógy,
- sťahovanie z URL,
- generovanie testovacích dát,
- indexovanie,
- načítanie viditeľných riadkov,
- navigáciu a scrollbar,
- vyhľadávanie,
- uloženie súboru,
- správu dočasných súborov,
- stavové a chybové hlásenia.

To sťažuje testovanie, zmenu UI a bezpečné riadenie asynchrónnych operácií.

## Ďalšie zistenia

- Indexer vyhľadáva znak nového riadku priamo v bajtoch. Tento prístup nie je spoľahlivý pre UTF-16 a ďalšie viacbajtové encodingy.
- URL download nemá progress, limit veľkosti ani používateľské zrušenie.
- Dočasné súbory sa nevytvárajú cez systémový temp mechanizmus a ich názov môže kolidovať.
- `async void` metódy komplikujú spracovanie chýb a testovanie.
- Projekt sa zostaví, ale aktuálne obsahuje nullable warningy v `RelayCommand` a vo vyhľadávaní.
- Chýbajú automatizované testy kritických scenárov.

## Odporúčaná cieľová architektúra

```text
MainViewModel
├── DocumentViewModel
│   ├── aktuálny súbor
│   ├── viditeľné riadky
│   └── stav indexovania
├── SearchViewModel
│   ├── query a nastavenia
│   ├── aktuálny výsledok
│   └── progress a cancellation
├── NavigationViewModel
│   ├── aktuálny riadok
│   └── viewport a scrollbar
└── služby
    ├── IFileIndexService
    ├── ITextPageProvider
    ├── ISearchService
    ├── IDownloadService
    ├── ITempFileService
    └── IFileDialogService
```

## Definícia úspechu

Prvá stabilná verzia je úspešná, ak:

- používateľ sa dostane na prvý aj posledný riadok bez straty obsahu,
- čísla riadkov zodpovedajú fyzickým riadkom súboru,
- text sa dá označiť a kopírovať,
- každý výsledok vyhľadávania sa dá nájsť a zvýrazniť,
- dlhé operácie zobrazujú stav a dajú sa zrušiť,
- otvorenie iného súboru alebo rýchly scroll nespôsobí zobrazenie zastaraných dát,
- kritické správanie je pokryté automatizovanými testami.

