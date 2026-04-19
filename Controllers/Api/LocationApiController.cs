 using Microsoft.AspNetCore.Mvc;

namespace PayroTech.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class LocationApiController : ControllerBase
{
    [HttpGet("provinces")]
    public IActionResult GetProvinces()
    {
        var provinces = new[]
        {
            new { code = "NCR", name = "Metro Manila", region = "NCR" },
            new { code = "ABR", name = "Abra", region = "CAR" },
            new { code = "AGN", name = "Agusan del Norte", region = "XIII" },
            new { code = "AGS", name = "Agusan del Sur", region = "XIII" },
            new { code = "AKL", name = "Aklan", region = "VI" },
            new { code = "ALB", name = "Albay", region = "V" },
            new { code = "ANT", name = "Antique", region = "VI" },
            new { code = "APA", name = "Apayao", region = "CAR" },
            new { code = "AUR", name = "Aurora", region = "III" },
            new { code = "BAS", name = "Basilan", region = "BARMM" },
            new { code = "BAN", name = "Bataan", region = "III" },
            new { code = "BTN", name = "Batanes", region = "II" },
            new { code = "BTG", name = "Batangas", region = "IV-A" },
            new { code = "BEN", name = "Benguet", region = "CAR" },
            new { code = "BIL", name = "Biliran", region = "VIII" },
            new { code = "BOH", name = "Bohol", region = "VII" },
            new { code = "BUK", name = "Bukidnon", region = "X" },
            new { code = "BUL", name = "Bulacan", region = "III" },
            new { code = "CAG", name = "Cagayan", region = "II" },
            new { code = "CAN", name = "Camarines Norte", region = "V" },
            new { code = "CAS", name = "Camarines Sur", region = "V" },
            new { code = "CAM", name = "Camiguin", region = "X" },
            new { code = "CAP", name = "Capiz", region = "VI" },
            new { code = "CAT", name = "Catanduanes", region = "V" },
            new { code = "CAV", name = "Cavite", region = "IV-A" },
            new { code = "CEB", name = "Cebu", region = "VII" },
            new { code = "COM", name = "Compostela Valley", region = "XI" },
            new { code = "DAO", name = "Davao del Norte", region = "XI" },
            new { code = "DAS", name = "Davao del Sur", region = "XI" },
            new { code = "DAC", name = "Davao de Oro", region = "XI" },
            new { code = "DAO", name = "Davao Oriental", region = "XI" },
            new { code = "DVO", name = "Davao Occidental", region = "XI" },
            new { code = "EAS", name = "Eastern Samar", region = "VIII" },
            new { code = "GUI", name = "Guimaras", region = "VI" },
            new { code = "IFU", name = "Ifugao", region = "CAR" },
            new { code = "ILN", name = "Ilocos Norte", region = "I" },
            new { code = "ILS", name = "Ilocos Sur", region = "I" },
            new { code = "ILO", name = "Iloilo", region = "VI" },
            new { code = "ISA", name = "Isabela", region = "II" },
            new { code = "KAL", name = "Kalinga", region = "CAR" },
            new { code = "LAG", name = "Laguna", region = "IV-A" },
            new { code = "LAN", name = "Lanao del Norte", region = "X" },
            new { code = "LAS", name = "Lanao del Sur", region = "BARMM" },
            new { code = "LUN", name = "La Union", region = "I" },
            new { code = "LEY", name = "Leyte", region = "VIII" },
            new { code = "MAG", name = "Maguindanao", region = "BARMM" },
            new { code = "MAD", name = "Marinduque", region = "IV-B" },
            new { code = "MAS", name = "Masbate", region = "V" },
            new { code = "MDC", name = "Mindoro Occidental", region = "IV-B" },
            new { code = "MDR", name = "Mindoro Oriental", region = "IV-B" },
            new { code = "MSC", name = "Misamis Occidental", region = "X" },
            new { code = "MSR", name = "Misamis Oriental", region = "X" },
            new { code = "MOU", name = "Mountain Province", region = "CAR" },
            new { code = "NEC", name = "Negros Occidental", region = "VI" },
            new { code = "NER", name = "Negros Oriental", region = "VII" },
            new { code = "NSA", name = "Northern Samar", region = "VIII" },
            new { code = "NUE", name = "Nueva Ecija", region = "III" },
            new { code = "NUV", name = "Nueva Vizcaya", region = "II" },
            new { code = "PAM", name = "Pampanga", region = "III" },
            new { code = "PAN", name = "Pangasinan", region = "I" },
            new { code = "PAL", name = "Palawan", region = "IV-B" },
            new { code = "QUE", name = "Quezon", region = "IV-A" },
            new { code = "QUI", name = "Quirino", region = "II" },
            new { code = "RIZ", name = "Rizal", region = "IV-A" },
            new { code = "ROM", name = "Romblon", region = "IV-B" },
            new { code = "SAR", name = "Samar", region = "VIII" },
            new { code = "SAG", name = "Sarangani", region = "XII" },
            new { code = "SIG", name = "Siquijor", region = "VII" },
            new { code = "SOR", name = "Sorsogon", region = "V" },
            new { code = "SCO", name = "South Cotabato", region = "XII" },
            new { code = "SLE", name = "Southern Leyte", region = "VIII" },
            new { code = "SUK", name = "Sultan Kudarat", region = "XII" },
            new { code = "SLU", name = "Sulu", region = "BARMM" },
            new { code = "SUN", name = "Surigao del Norte", region = "XIII" },
            new { code = "SUR", name = "Surigao del Sur", region = "XIII" },
            new { code = "TAR", name = "Tarlac", region = "III" },
            new { code = "TAW", name = "Tawi-Tawi", region = "BARMM" },
            new { code = "ZMB", name = "Zambales", region = "III" },
            new { code = "ZAN", name = "Zamboanga del Norte", region = "IX" },
            new { code = "ZAS", name = "Zamboanga del Sur", region = "IX" },
            new { code = "ZSI", name = "Zamboanga Sibugay", region = "IX" }
        };

        return Ok(provinces);
    }

    [HttpGet("cities/{provinceCode}")]
    public IActionResult GetCities(string provinceCode)
    {
        var cities = new Dictionary<string, string[]>
        {
            ["NCR"] = new[] { "Caloocan", "Las Piñas", "Makati", "Malabon", "Mandaluyong", "Manila", "Marikina", "Muntinlupa", "Navotas", "Parañaque", "Pasay", "Pasig", "Quezon City", "San Juan", "Taguig", "Valenzuela", "Pateros" },
            ["CAV"] = new[] { "Cavite City", "Bacoor", "Dasmariñas", "Imus", "Tagaytay", "Trece Martires", "General Trias", "Silang", "Tanza" },
            ["LAG"] = new[] { "Calamba", "San Pablo", "Biñan", "Santa Rosa", "Cabuyao", "San Pedro", "Los Baños", "Bay", "Calauan" },
            ["BTG"] = new[] { "Batangas City", "Lipa", "Tanauan", "Santo Tomas", "Lemery", "Taal", "Balayan", "Nasugbu" },
            ["RIZ"] = new[] { "Antipolo", "Cainta", "Taytay", "Binangonan", "San Mateo", "Rodriguez", "Morong", "Teresa" },
            ["BUL"] = new[] { "Malolos", "Meycauayan", "San Jose del Monte", "Marilao", "Bocaue", "Balagtas", "Guiguinto", "Sta. Maria" },
            ["PAM"] = new[] { "Angeles", "San Fernando", "Mabalacat", "Porac", "Mexico", "Magalang", "Arayat", "Guagua" },
            ["CEB"] = new[] { "Cebu City", "Mandaue", "Lapu-Lapu", "Talisay", "Toledo", "Danao", "Carcar", "Naga" },
            ["DAS"] = new[] { "Davao City", "Digos City", "Bansalan", "Hagonoy", "Kiblawan", "Magsaysay", "Malalag", "Matanao", "Padada", "Santa Cruz", "Sulop" }
        };

        if (cities.ContainsKey(provinceCode.ToUpper()))
        {
            return Ok(cities[provinceCode.ToUpper()]);
        }

        return Ok(new string[] { });
    }

    [HttpGet("barangays/{provinceCode}/{city}")]
    public IActionResult GetBarangays(string provinceCode, string city)
    {
        // Comprehensive barangays for Davao del Sur
        var barangays = new Dictionary<string, string[]>
        {
            // Metro Manila
            ["Manila"] = new[] { "Ermita", "Intramuros", "Malate", "Paco", "Pandacan", "Port Area", "Quiapo", "Sampaloc", "San Miguel", "Santa Ana", "Santa Cruz", "Tondo" },
            ["Quezon City"] = new[] { "Bagong Pag-asa", "Batasan Hills", "Commonwealth", "Cubao", "Diliman", "Fairview", "Kamuning", "Libis", "Novaliches", "Project 4", "Tandang Sora", "UP Campus" },
            ["Makati"] = new[] { "Bel-Air", "Dasmariñas", "Forbes Park", "Guadalupe Nuevo", "Guadalupe Viejo", "Magallanes", "Poblacion", "Rockwell", "Salcedo", "San Lorenzo", "Urdaneta", "Valenzuela" },
            ["Pasig"] = new[] { "Bagong Ilog", "Kapitolyo", "Manggahan", "Maybunga", "Ortigas", "Pinagbuhatan", "Rosario", "Sagad", "San Antonio", "San Joaquin", "Santolan", "Ugong" },
            
            // Davao City - Complete 182 barangays
            ["Davao City"] = new[] {
                // Poblacion Districts
                "1-A (Poblacion)", "2-A", "3-A", "4-A", "5-A", "6-A", "7-A", "8-A", "9-A", "10-A",
                "11-B", "12-B", "13-B", "14-B", "15-B", "16-B", "17-B", "18-B", "19-B", "20-B",
                "21-C", "22-C", "23-C", "24-C", "25-C", "26-C", "27-C", "28-C", "29-C", "30-C",
                "31-D", "32-D", "33-D", "34-D", "35-D", "36-D", "37-D", "38-D", "39-D", "40-D",
                // Talomo District
                "Bago Aplaya", "Bago Gallera", "Baliok", "Bucana", "Catalunan Grande", "Catalunan Pequeno",
                "Dumoy", "Langub", "Maa", "Magtuod", "Matina Aplaya", "Matina Crossing", "Matina Pangi", "Talomo Proper",
                // Agdao District
                "Agdao Proper", "Wilfredo Aquino", "Centro (San Juan)", "Lapu-Lapu", "Leon Garcia", "Tomas Monteverde",
                "Paciano Bangoy", "Rafael Castillo", "San Antonio", "Ubalde", "Vicente Duterte",
                // Buhangin District
                "Acacia", "Angliongto", "Buhangin Proper", "Cabantian", "Callawa", "Communal", "Hizon", "Indangan",
                "Mandug", "Pampanga", "Sasa", "Tigatto", "Waan",
                // Bunawan District
                "Bunawan Proper", "Gatungan", "Ilang", "Lasang", "Mahayag", "Mudiang", "Panacan", "San Isidro (Bunawan)", "Tibungco",
                // Paquibato District
                "Colosas", "Fatima (Paquibato)", "Lumiad", "Mabuhay", "Malabog", "Mapula", "Panalum", "Pandaitan",
                "Paquibato Proper", "Paradise Embac", "Salapawan", "Sumimao", "Tapak",
                // Baguio District
                "Baguio Proper", "Cadalian", "Carmen", "Gumalang", "Malagos", "Tambobong", "Tawan-Tawan", "Wines",
                // Calinan District
                "Biao Joaquin", "Calinan Poblacion", "Cawayan", "Dacudao", "Dalagdag", "Dominga", "Inayangan", "Lacson",
                "Lamanan", "Lampianao", "Megcawayan", "Pangyan", "Riverside", "Saloy", "Sirib", "Subasta", "Talomo River",
                "Tamayong", "Wangan",
                // Marilog District
                "Baganihan", "Bantol", "Buda", "Dalag", "Datu Salumay", "Gumitan", "Magsaysay (Marilog)", "Malamba",
                "Marilog Proper", "Salaysay", "Suawan", "Tamugan",
                // Toril District
                "Alambre", "Atan-Awe", "Bankas Heights", "Baracatan", "Bato", "Bayabas", "Binugao", "Camansi", "Catigan",
                "Crossing Bayabas", "Daliao", "Daliaon Plantation", "Eden", "Kilate", "Lizada", "Lubogan", "Marapangi",
                "Mulig", "Sibulan", "Sirawan", "Tagluno", "Tagurano", "Tibuloy", "Toril Proper", "Tungkalan",
                // Tugbok District
                "Alambre", "Bago Oshiro", "Balingaeng", "Biao Escuela", "Biao Guianga", "Los Amigos", "Manambulan",
                "Manuel Guianga", "Matina Biao", "Mintal", "New Carmen", "New Valencia", "Sto. Nino (Tugbok)", "Tacunan",
                "Tagakpan", "Talandang", "Tugbok Proper", "Ula"
            },
            
            // Digos City
            ["Digos City"] = new[] {
                "Aplaya", "Balabag", "Binaton", "Cogon", "Colorado", "Dawis", "Dulangan", "Goma", "Igpit",
                "Kiagot", "Lungag", "Mahayahay", "Matti", "Ruparan", "San Agustin", "San Jose (Digos)", "San Miguel (Digos)",
                "San Roque", "Sinawilan", "Soong", "Tiguman", "Tres de Mayo", "Zone 1 (Poblacion)", "Zone 2 (Poblacion)", "Zone 3 (Poblacion)"
            },
            
            // Bansalan
            ["Bansalan"] = new[] {
                "Alegre", "Alta Vista", "Anonang", "Bitaug", "Bonifacio", "Buenavista", "Darapuay", "Dolo",
                "Eman", "Kinuskusan", "Libertad", "Linawan", "Mabuhay", "Mabunga", "Managa", "Marber",
                "New Clarin", "Poblacion (Bansalan)", "Rizal", "Santo Nino", "Sibayan", "Tinongtongan", "Tubod", "Union", "Waterfall"
            },
            
            // Hagonoy
            ["Hagonoy"] = new[] {
                "Aplaya", "Balutakay", "Clib", "Guihing", "Hagonoy Crossing", "Hagonoy Proper", "Hagonoy Poblacion",
                "Kanapulo", "La Union", "Lanuro", "Lapulabao", "Leling", "Lower Bala", "Mahayahay", "Malabang",
                "New Quezon", "Paligue", "Sacub", "San Guillermo", "Sinayawan", "Upper Bala"
            },
            
            // Kiblawan
            ["Kiblawan"] = new[] {
                "Abnate", "Bagumbayan", "Balasiao", "Bonifacio", "Bunot", "Cogon", "Dapok", "Ihan", "Kimlawis",
                "Kisulan", "Lati-an", "Mabuhay", "Maraga-a", "Molopolo", "New Sibonga", "Panaglib", "Pasig",
                "Poblacion (Kiblawan)", "Pocaleel", "Pongpong", "Saboy", "San Jose", "San Manuel", "San Pedro",
                "Tacub", "Tacul", "Taguibo", "Tuban", "Waterfall", "Bagong Negros"
            },
            
            // Magsaysay
            ["Magsaysay"] = new[] {
                "Bacungan", "Balnate", "Barayong", "Colonsabak", "Dalumay", "Dolo", "Kanapulo", "Kasuga",
                "Lower Bala", "Lower Limonzo", "Mabini", "Malawanit", "Malongon", "New Ilocos", "New Visayas",
                "Poblacion (Magsaysay)", "San Isidro", "San Miguel", "Tacul", "Tagaytay", "Upper Bala", "Upper Limonzo"
            },
            
            // Malalag
            ["Malalag"] = new[] {
                "Bagumbayan", "Bolton", "Bulacan", "Caputian", "Guadalupe", "Kiblagon", "Mabini", "New Baclayon",
                "Pitu", "Poblacion (Malalag)", "Tagansule", "Tagbac", "Tala-o", "Tamlangon", "Tubod"
            },
            
            // Matanao
            ["Matanao"] = new[] {
                "Asbang", "Asinan", "Bagumbayan", "Bangkal", "Buas", "Buri", "Camanchiles", "Cabligan", "Colonsabak",
                "Dongan-Pekong", "Kabasagan", "Kapok", "Kibao", "La Suerte", "Lower Marber", "Manga", "New Katipunan",
                "New Murcia", "New Visayas", "Poblacion (Matanao)", "Saboy", "San Jose", "San Miguel", "San Vicente",
                "Savannah", "Sinaragan", "Sinawilan", "Tamlangon", "Tibongbong", "Towak", "Upper Klinan", "Upper Marber", "Zion"
            },
            
            // Padada
            ["Padada"] = new[] {
                "Almendras", "Don Sergio Osmeña Sr.", "Harada Butai", "La Suerte", "Lower Katipunan", "Lower Limonzo",
                "Mabini", "Magsaysay", "Northern Paligue", "Palili", "Piape", "Poblacion (Padada)", "Punta Piape",
                "Southern Paligue", "Tulogan", "Tulongan", "Upper Limonzo"
            },
            
            // Santa Cruz
            ["Santa Cruz"] = new[] {
                "Astorga", "Bato", "Coronon", "Darong", "Inawayan", "Jose Rizal", "Matutungan", "Poblacion (Santa Cruz)",
                "Sibulan", "Tagabuli", "Tibolo", "Tuban", "Bato Batong", "Dapok", "Inawayan Proper", "Jose Abad Santos",
                "Matutungan Proper", "Sibulan Proper"
            },
            
            // Sulop
            ["Sulop"] = new[] {
                "Balasinon", "Buguis", "Carre", "Harada Butai", "Kiblagon", "Labon", "Laperas", "Lapla",
                "Litos", "Luparan", "Mckinley", "Osmeña", "Parame", "Poblacion (Sulop)", "Roxas", "Solongvale",
                "Tagolilong", "Tala-o", "Talas", "Tanwalang", "Tawing", "Tala-o Proper", "Talas Proper", "Tanwalang Proper", "Tawing Proper"
            }
        };

        if (barangays.ContainsKey(city))
        {
            return Ok(barangays[city]);
        }

        return Ok(new string[] { });
    }
}
