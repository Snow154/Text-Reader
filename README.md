# Text Reader App

Aplikacia Text Reader je aplikacia, ktora dokaze citat vacsie textove subory. Pouzil som na jej tvorbu WPF (taktiez podmienkou by mal byt nainstalovany .NET 8 Desktop Runtime), kde aplikacia vyuziva len jedno MainWindow okno. Pomocou tlacidiel Load File, Random a Load URL dokaze nacitat dane data z vasho pozadovaneho zdroja. Aplikaciu som testoval s rozne velkymi textovymi subormi. V aplikacii mozme len subory citat. Hlavny core funkcnosti aplikacie je jeho vizualizacia a indexovanie. 

Pri nacitani noveho suboru z lokalneho disku sa subor otvori len v preview mode kedy vidime zaciatok suboru ale zvysok suboru sa indexuje na pozadi. Po dokonceni indexovania je mozne celym suborom scrollovat. Na vykreslenie textu a scrollovanie vyuzivam Text Block, no TextBlock neobsahuje cely nacitany subor ale len vizualizuje isty pocet riadkov a pocas scrollovania sa zvysny text nacita. 

Vyuzitie TextBlocku je kvoli tomu ze vdaka line virtualizacii a zobrazovani len isteho poctu riadkov, je po testovani aj s 10GB textovym suborom plynule. Cize nerenderuje plne cely text file naraz ale virtualizuje postupne istu cast textu.  

Aplikacia nacita tieto typy lokalnych suborov :

*.txt, *.log, *.csv, *.json, *.xml, *.html a All files (\*.\*)

URL funkcionalita pracuje stiahnutim si html textu zo zadaneho linku a nasledne jeho ulozenim v temp subore ktory sa pri zavreti aplikacie vymaze.

Generate Random vytvori random sekvenciu pismen, slov a nahodny pocet riadkov. Moze ich byt od 500 tisic po 1 milion. 

Stlacenim CTRL + F sa objavi vyhladavaci textbox, ktore mozeme ovladat aj tlacidlami next/previous ale taktiez aj klavesami ako boli opisane v zadani. 

### Pred pouzitim 
Je potrebne mat nainstalovany .NET 8 Desktop Runtime.

## Struktura projektu

- **MainWindow.xaml** obsahuje GUI funkcionalitu hlavneho okna - vizual a taktiez eventy danych UI elementov. 
- **MVVM/ViewModel/MainViewModel.cs** - obsahuje hlavnu funkcionalitu - Ovladanie jednotlivych funkcionalit tlacidiel, nacitanie dat (generovanie random textu, ziskanie textu z URL, nacitanie lokalneho suboru)
- **MVVM/Utility/RelayCommand.cs** - Utility class, na funkcionalitu buttonov v GUI
- **MVVM/Services/FileIndexer.cs** - Service, ktory ma hlavnu ulohu indexovat subor pri jeho nacitani, vytvori jednotlive indexy na zaklade citania suboru po bytoch(1 MB)
- **MVVM/Services/TextProvider.cs** - Service, ktory ziskava jednotlive riadky zo suboru na zaklade indexov vytvorenych vo FileIndexer.cs
- **MVVM/Model/*** - obsahuje jednotlive datove struktury vyuzite v aplikacii