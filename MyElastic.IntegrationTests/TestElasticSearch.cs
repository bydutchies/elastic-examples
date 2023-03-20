using Nest;

namespace MyElastic.IntegrationTests;

[TestClass]
public class TestElasticClient
{
  private const string DEFAULT_INDEX = "bydutchies-posts";
  private readonly IElasticClient _client;

  public TestElasticClient()
  {
    var connectionSettings = new ConnectionSettings(new Uri("<endpoint>"))
        .DefaultIndex(DEFAULT_INDEX)
        .BasicAuthentication("<username>", "<password>");

    _client = new ElasticClient(connectionSettings);
  }

  [TestMethod]
  public async Task CreateIndex()
  {
    var indexExistsResponse = await _client.Indices.ExistsAsync(new IndexExistsRequest(DEFAULT_INDEX));
    if (!indexExistsResponse.Exists)
    {
      var response = await _client.Indices.CreateAsync(DEFAULT_INDEX, c => c
        .Settings(s => s
              .Analysis(a => a
                  .TokenFilters(tf => tf
                      .Stop("dutch_stop", st => st
                        .StopWords("_dutch_")
                      )
                      .Stemmer("dutch_stemmer", st => st
                          .Language("dutch")
                      )
                      .Synonym("my_synonyms", st => st
                          .Synonyms(
                            "penetratietest,pentest")
                      )
                      .Phonetic("my_phonetic", st => st
                        .Encoder(PhoneticEncoder.Beidermorse) // More language independent
                        .LanguageSet(PhoneticLanguage.Any)
                      )
                  )
                  .Analyzers(aa => aa
                      .Custom("full_dutch", ca => ca
                          .Tokenizer("standard")
                          .Filters("lowercase",
                                   "dutch_stop",
                                   "dutch_stemmer",
                                   "my_synonyms",
                                   "my_phonetic")
                      )
                  )
              )
          )
          .Map<Post>(m => m
              .AutoMap()
              .Properties(p => p
                  .Text(t => t
                      .Name(n => n.Title)
                      .Analyzer("full_dutch")
                  )
                  .Text(t => t
                      .Name(n => n.Body)
                      .Analyzer("full_dutch")
                  )
              )
          ));
    }
  }

  [TestMethod]
  public async Task DeleteIndex()
  {
    var indexExists = await _client.Indices.ExistsAsync(new IndexExistsRequest(DEFAULT_INDEX));
    if (indexExists.Exists)
    {
      var deleteIndexRequest = new DeleteIndexRequest(DEFAULT_INDEX);
      await _client.Indices.DeleteAsync(deleteIndexRequest);
    }
  }

  [TestMethod]
  public async Task IndexItems()
  {
    var books = new List<Post>()
    {
        new Post
        {
            Id = 1,
            Title = "Alternatieve middleware in Azure Functions",
            Url = "/posts/alternatieve-middleware-in-azure-functions",
            Body = "Helaas heb je in een Azure Function geen mogelijkheid om in te haken op de request pipeline voordat de functie daadwerkelijk wordt gestart. Hierdoor ben je gedwongen om zelf een oplossing te schrijven voor code die wordt gedeeld tussen de functies, zoals bijvoorbeeld de foutafhandeling of autorisatie. Wij maken gebruik van een helper class om de gedeelde code op een gestructureerde manier op te zetten. Daarbij hebben we ook volledige controle over de volgorde waarin deze wordt uitgevoerd voordat de code van de functie daadwerkelijk wordt gestart. De helper class in mijn voorbeeld bevat code voor zowel autorisatie als een uniforme foutafhandeling voor een http trigger. De 'truc' is hier in ieder geval de anonieme functie die wordt meegegeven naar de methode in de helper class. Dit is de daadwerkelijke code van de specifieke functie die uitgevoerd wordt. Dit wordt duidelijk als je ziet hoe de helper wordt toegepast bij de functie. Op deze manier hebben we de volgende zaken op uniforme wijze voor alle http triggers opgelost: - Autorisatie - Request context - Foutafhandeling Mocht je een grote aanhanger zijn van middleware in de traditionele ASP.Net applicaties, dan kun je de post van Divakar Kumar wel waarderen. Hij is een stap verder gegaan om dezelfde middleware ervaring te creeren. Voor mij was dit te veel overhead, dus ik blijf bij de oplossing met de helper class.\r\n",
            Author = "Wilco",
        },
        new Post
        {
            Id = 2,
            Title = "Van entiteit naar dto met AutoMapper",
            Url = "/posts/van-entiteit-naar-dto-met-automapper",
            Body = "API's moeten veel transformaties uitvoeren door entiteiten uit de business logica te vertalen naar Data Transfer Objects (dto's), waarmee wordt gecommuniceerd met de client. Daarbij is het handig om op een uniforme manier transformaties te kunnen uitvoeren voor objecten. Als je op zoek gaat naar de beste libraries die op dit moment op de markt beschikbaar zijn, dan kom al snel uit bij AutoMapper. Met Automapper zijn we tot nu toe nog geen transformatie tegen gekomen die niet met deze mapper is op te lossen. Wat is AutoMapper? AutoMapper is een C# library die object transformatie mogelijk maakt op basis van een vooraf vastgelegde mapping configuratie. Het voordeel van AutoMapper is de eenmalige configuratie, waarna je met één regel code een object (of lijst met objecten) kunt transformeren. Hoe voeg ik AutoMapper toe aan mijn .Net6 applicatie? De eerste stap om AutoMapper te kunnen gebruiken in je applicatie is het toevoegen van een nuget package AutoMapper.Extensions.Microsoft.DependencyInjection. Vervolgens dient AutoMapper te worden toegevoegd aan de service collectie. Dat doe je door de volgende regel op te nemen in de startup class van je applicatie: DIt zorgt er niet alleen voor dat AutoMapper wordt toegevoegd aan de service collectie, maar ook alle mapping configuratiebestanden in de geladen assemblies worden ingelezen. Vervolgens kun je in de services gebruik van van AutoMapper via dependency injection. Hoe voeg ik mapping configuratie toe? Mapping configuratie kun je toevoegen door een class te definiëren die erft van Automapper.Profile. Vervolgens leg je vast voor welke objecttransformatie deze configuratie is en hoe de properties aan elkaar moeten worden gemapt. Dit kan van simpel tot meer complexe transformaties. Ik zal het toelichten aan de hand van een simpel voorbeeld. Neem de volgende twee classes: De configuratie is heel makkelijk. Bij conventie worden de properties automatisch gemapt als ze van hetzelfde type zijn en dezelfde naam hebben. De configuratie om Address naar AdressDto om te zetten ziet er als volgt uit: Alle configuratie is nu gereed om met één regel code object transformatie van Adress toe te passen in je applicatie. Dit gaat net zo makkelijk voor een lijst met objecten. Ik heb een voorbeeld project gemaakt die beschikbaar is via on Github account. Dan kun je alle voorbeelden zelf uitproberen. De belangrijkste configuratiemogelijkheden Mocht de naamgeving niet gelijk zijn kun je in de configuratie aangeven welke properties met elkaar gemapt moeten worden. Ook handig is de ReverseMap() methode. Door deze op te nemen in je configuratie is het mogelijk om van Address naar AdressDto te transformeren en de andere kant op, van AddressDto naar Address. Het is ook mogelijk om condities in te stellen. Hieraan moet worden voldaan voordat de property gemapt wordt. Dit kan worden gerealiseerd door gebruik te maken van precondities. In een aantal gevallen kom je niet weg met het mappen van properties. In deze gevallen is er wat meer complexe mapping nodig. Denk hierbij aan een berekening of een service die aangeroepen moet worden. Voor deze gevallen zijn Resolvers bedacht. Je kunt een eigen Resolver schrijven door een class te definiëren en de Automapper.IValueResolver interface te implementeren. En vervolgens kun je de Resolver als volgt opnemen in de configuratie: We hebben de belangrijkste mogelijkheden van Automapper nu besproken. Ik wil nogmaals verwijzen naar ons Github account waar ik voorbeelden van alle besproken mogelijkheden heb toegevoegd. Mocht je op zoek zijn naar alle mogelijkheden van Automapper dan wil ik je naar de officiële documentatie verwijzen.",
            Author = "Wilco",
        },
        new Post
        {
            Id = 3,
            Title = "Routering ordenen",
            Url = "/posts/routering-ordenen",
            Body = "De routering van Azure Function wijkt af van de routering van een traditionele ASP.Net applicatie.  Neem de volgende endpoints: Je zou terecht verwachten dat een request voor /api/users/current naar het tweede endpoint wordt gestuurd en een request voor /api/users/1 naar het eerste endpoint. De praktijk is dat je dat niet met zekerheid kunt zeggen. Wij gebruiken Azure Functions App ook als API en zijn vaak tegen onverwachte resultaten aangelopen. Ik kwam dit artikel tegen van Brian Dunnington. Hij beschrijft daarin hoe de routering werkt en zijn onderzoek naar mogelijkheden om de routering aan te passen. Ik zal niet herhalen hoe zijn onderzoek is verlopen, maar hij heeft uiteindelijk wel een oplossing gevonden. Door gebruik te maken van een WebJobsBuilderExtensions heeft hij een manier gevonden om tijdens het starten van de applicatie in te haken op de routing collection. Het resultaat is een startup class die je op kunt nemen in de root van je Function App. Deze kan gewoon naast de functions startup class worden opgenomen, als deze aanwezig is. Het resultaat komt aardig in de buurt van de routering van een traditionele ASP.Net applicatie, maar er zijn een paar verschillen. Voor mij geen probleem, dus een prima oplossing! ",
            Author = "Wilco",
        },
        new Post
        {
            Id = 4,
            Title = "Uitzetten opstartscherm Azure Function App",
            Url = "/posts/uitzetten-opstartscherm-azure-function-app",
            Body = "Een veelgestelde vraag die ik vaak hoor is: Hoe zet je het opstartscherm van een Azure Function App uit? De achterliggende reden voor die vraag zou kunnen zijn dat je geen informatie over je infrastructuur naar buiten bekend wilt maken. Ook kwam deze pagina in een penetratietest naar voren. Het blijkt dat Microsoft hier een verouderde javascript library gebruikt die onveilig wordt beschouwd.  Het antwoord op de vraag is simpel. Dit kun je doen door een environment variable 'AzureWebJobsDisableHomepage' toe te voegen en de waarde 'true' mee te geven. Om dit lokaal te testen kun je de launchSettings aanpassen. Voor een Function App die is uitgerold naar Azure is het mogelijk via de Azure Portal. Ga naar de Function App en selecteer 'Configuration'. Voeg daarna de instelling toe zoals hieronder is weergegeven. Het handmatig zetten van de instelling in de Azure Portal zal normaal gesproken niet vaak voorkomen. In een omgeving waarin de infrastructuur geautomatiseerd wordt aangemaakt heeft het de voorkeur omdat de instelling wordt toegevoegd een ARM template of bicep file waarin de Azure Function App wordt aangemaakt.",
            Author = "Wilco",
        },
        new Post
        {
            Id = 5,
            Title = "Applicatie instellingen per omgeving",
            Url = "/posts/applicatie-instellingen-per-omgeving",
            Body = "In traditionele ASP.Net applicaties is het gebruikelijk om te werken met applicatie instellingen per omgeving. De bekende appsettings,json waarin de algemene instellingen staan en een apart bestand per omgeving waarin de omgeving specifieke instellingen zijn opgenomen, zoals bijvoorbeeld appsettings.development.json. Zo kun je voor elke omgeving in de OTAP straat aparte instellingen definiëren. Wanneer je een nieuwe Function App aanmaakt ontbreken deze bestanden, maar er is een manier om deze toch toe te voegen. Daarvoor moet je eerst een nuget package laden, namelijk Microsoft.Azure.Functions.Extensions. Je hebt nu de mogelijkheid om een startup class toe te voegen aan het project. Mijn startup class ziet er als volgt uit: Je ziet dat op basis van omgevingsvariabele ASPNETCORE_ENVIRONMENT de instellingen van een specifieke omgeving worden ingelezen. De applicatie instellingen bestanden dienen in de root van de applicatie aanwezig te zijn. Zorg ervoor dat je bij de eigenschappen van deze bestanden 'Copy to output directory' op 'Copy always' zet. De local.host.json kun je verwijderen. Daarmee zijn we er nog niet helemaal. Je wilt secrets als een connectiestring voor je database of storage account niet opnemen in de git repository. Wij hebben ervoor gekozen om voor lokale ontwikkeling gebruik te maken van de secrets manager in Visual Studio. Voor een omgeving die is uitgerold naar Azure worden de secrets via de release pipeline opgenomen in de app settings van de App Service. In dit artikel van Faizaan Shaikh wordt uitgelegd hoe je secrets manager kunt gebruiken in Visual Studio. Zijn demo loopt stap voor stap door het proces. ij gebruiken voor alle services en web applicaties binnen het platform hetzelfde secrets bestand. Dit kun je bereiken door in de projectinstellingen de eigenschap 'UserSecretsId' overal gelijk te maken. Als programmeur hoef je het secrets bestand maar eenmalig in te stellen en heb je de secrets in alle applicaties tot je beschikking.",
            Author = "Wilco",
        },
        new Post
        {
            Id = 6,
            Title = "Beveiligingsheaders instellen ",
            Url = "/posts/beveiligingsheaders-instellen",
            Body = "Uit de resultaten van een penetratietest op een applicatie kwam naar voren dat er beveiligingsheaders ontbraken. Het eerste wat je als ontwikkelaar natuurlijk doet is checken of de test wel klopt. Dat kan in dit geval makkelijk via de website securityheaders.com. Helaas liet het resultaat niet veel ruimte voor discussie. Wat zijn HTTP Security Headers eigenlijk? Wanneer je een webserver aanroept geeft deze jou een mix van onder andere content en HTTP response headers. In deze response headers staan allerlei instellingen waarmee tegen de aanroepende applicatie verteld wordt hoe die met de content moet omgaan. Zoals het testresultaat al duidelijk maakt zijn er een aantal die belangrijk zijn: Strict-Transport-Security De HTTP Strict Transport Security zorgt er voor dat er alleen via HTTPS gecommuniceerd mag worden. Dit verzekert dat de verbinding veilig is. Content-Security-Policy Met de Content Security Policy definieer je van welke goedgekeurde bronnen de browser van de bezoeker bestanden (stylesheets, Javascript etc.) mag laden. Je kunt ervoor kiezen om alle externe bronnen op jouw eigen site te plaatsen, of via de CSP te regelen welke externe bronnen je accepteert. X-Frame-Options De X-Frame-Options zorgt er voor dat website niet binnen een iframe kan draaien op een andere website. Een mogelijk veiligheidsprobleem kan zijn dat er dan over je website heen een laag gelegd wordt, waarbij over de knoppen van jouw pagina andere knoppen overheen worden geplaatst en zo het gedrag kunnen beïnloeden.X-Content-Type-Options Met het zetten van de X-Content-Type-Options kan worden voorkomen dat bestanden anders worden gelezen. Bijvoorbeeld een php-script dat de extensie .jpg heeft en toch uitgevoerd wordt als php-script. Referrer-Policy De Referrer Policy zorgt er voor dat er ingesteld kan worden welke gegevens meegestuurd worden wanneer men op een externe link klikt. Permissions-Policy Met de Permissions-Policy bepaal je welke browserfuncties en functies je in of uit wilt schakelen. Denk hierbij aan gebruik van locatiebepaling, microfoon en camera. Hoe voeg je deze headers toe? De applicatie waar het testresultaat betekking op had is een website die ik host in een Azure Function App. Het hosten Is een leuk experiment, maar daar schrijf ik later nog een uitgebreid artikel over. Ik ben via Google op zoek gegaan naar mogelijke oplossingen voor een Azure Function App. De meeste resultaten gingen voornamelijk over het bewerken van de respons tijdens het uitvoeren van de functie. Dit vereist programmeerwerk en dat is niet wat je wilt. Een artikel van Saitej Kuralla bracht mij uiteindelijk op het juiste spoor. In de host.json heb je de mogelijkheid om 'customheaders' toe te voegen. Dit is de enige plek waar configuratie mogelijk is die voor alle functies geldt. Ik ben gestart met de boel zoveel mogelijk dicht te zetten, maar toen merkte ik dat de website niet meer optimaal functioneerde. Ik moest nog wat uitzonderingen definieren op in de Content-Security-Policy en toen werkte het weer. Mijn uiteindelijke aanpassing ziet er als volgt uit: Nog even controleren of we nu wel door de test heen komen. Precies zoals we hem willen hebben!",
            Author = "Wilco",
        }
    };
    var response = await _client.IndexManyAsync(books);

    Assert.IsNotNull(response);
  }
  
  [TestMethod]
  public async Task ListItems()
  {
    var searchResponse = _client.Search<Post>(s => s
        .Query(q => q
             .MatchAll()
        )
    );

    var posts = searchResponse.Documents;

    Assert.IsNotNull(posts);
  }

  [TestMethod]
  public async Task SearchItems()
  {
    var q = "pentest";

    var response = await _client.SearchAsync<Post>(s =>
            s.Query(sq =>
                sq.MultiMatch(mm => mm
                    .Query(q)
                    //.Fuzziness(Fuzziness.Auto)
                )
            )
        );

    var posts = response.Documents;

    Assert.IsNotNull(posts);
  }
}
