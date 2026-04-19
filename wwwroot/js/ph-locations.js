/**
 * Philippine Provinces and Cities Data
 * For cascading dropdown in company registration
 */
window.PHLocations = {
    regions: {
        "NCR": "National Capital Region",
        "CAR": "Cordillera Administrative Region",
        "I": "Ilocos Region",
        "II": "Cagayan Valley",
        "III": "Central Luzon",
        "IV-A": "CALABARZON",
        "IV-B": "MIMAROPA",
        "V": "Bicol Region",
        "VI": "Western Visayas",
        "VII": "Central Visayas",
        "VIII": "Eastern Visayas",
        "IX": "Zamboanga Peninsula",
        "X": "Northern Mindanao",
        "XI": "Davao Region",
        "XII": "SOCCSKSARGEN",
        "XIII": "Caraga",
        "BARMM": "Bangsamoro"
    },
    provinces: [
        { code: "NCR", name: "Metro Manila", region: "NCR" },
        { code: "ABR", name: "Abra", region: "CAR" },
        { code: "AGN", name: "Agusan del Norte", region: "XIII" },
        { code: "AGS", name: "Agusan del Sur", region: "XIII" },
        { code: "AKL", name: "Aklan", region: "VI" },
        { code: "ALB", name: "Albay", region: "V" },
        { code: "ANT", name: "Antique", region: "VI" },
        { code: "APA", name: "Apayao", region: "CAR" },
        { code: "AUR", name: "Aurora", region: "III" },
        { code: "BAS", name: "Basilan", region: "BARMM" },
        { code: "BAN", name: "Bataan", region: "III" },
        { code: "BTN", name: "Batanes", region: "II" },
        { code: "BTG", name: "Batangas", region: "IV-A" },
        { code: "BEN", name: "Benguet", region: "CAR" },
        { code: "BIL", name: "Biliran", region: "VIII" },
        { code: "BOH", name: "Bohol", region: "VII" },
        { code: "BUK", name: "Bukidnon", region: "X" },
        { code: "BUL", name: "Bulacan", region: "III" },
        { code: "CAG", name: "Cagayan", region: "II" },
        { code: "CAN", name: "Camarines Norte", region: "V" },
        { code: "CAS", name: "Camarines Sur", region: "V" },
        { code: "CAM", name: "Camiguin", region: "X" },
        { code: "CAP", name: "Capiz", region: "VI" },
        { code: "CAT", name: "Catanduanes", region: "V" },
        { code: "CAV", name: "Cavite", region: "IV-A" },
        { code: "CEB", name: "Cebu", region: "VII" },
        { code: "COM", name: "Cotabato", region: "XII" },
        { code: "DAV", name: "Davao de Oro", region: "XI" },
        { code: "DAS", name: "Davao del Sur", region: "XI" },
        { code: "DAO", name: "Davao Oriental", region: "XI" },
        { code: "DIN", name: "Dinagat Islands", region: "XIII" },
        { code: "EAS", name: "Eastern Samar", region: "VIII" },
        { code: "GUI", name: "Guimaras", region: "VI" },
        { code: "IFU", name: "Ifugao", region: "CAR" },
        { code: "ILN", name: "Ilocos Norte", region: "I" },
        { code: "ILS", name: "Ilocos Sur", region: "I" },
        { code: "ILO", name: "Iloilo", region: "VI" },
        { code: "ISA", name: "Isabela", region: "II" },
        { code: "KAL", name: "Kalinga", region: "CAR" },
        { code: "LAG", name: "Laguna", region: "IV-A" },
        { code: "LAN", name: "Lanao del Norte", region: "X" },
        { code: "LAS", name: "Lanao del Sur", region: "BARMM" },
        { code: "LUN", name: "La Union", region: "I" },
        { code: "LEY", name: "Leyte", region: "VIII" },
        { code: "MAG", name: "Maguindanao", region: "BARMM" },
        { code: "MRN", name: "Marinduque", region: "IV-B" },
        { code: "MSC", name: "Masbate", region: "V" },
        { code: "MSR", name: "Misamis Occidental", region: "X" },
        { code: "MSO", name: "Misamis Oriental", region: "X" },
        { code: "MOP", name: "Mountain Province", region: "CAR" },
        { code: "NEC", name: "Negros Occidental", region: "VI" },
        { code: "NER", name: "Negros Oriental", region: "VII" },
        { code: "NSA", name: "Northern Samar", region: "VIII" },
        { code: "NUE", name: "Nueva Ecija", region: "III" },
        { code: "NUV", name: "Nueva Vizcaya", region: "II" },
        { code: "MDC", name: "Occidental Mindoro", region: "IV-B" },
        { code: "MDR", name: "Oriental Mindoro", region: "IV-B" },
        { code: "PLW", name: "Palawan", region: "IV-B" },
        { code: "PAM", name: "Pampanga", region: "III" },
        { code: "PAN", name: "Pangasinan", region: "I" },
        { code: "QUE", name: "Quezon", region: "IV-A" },
        { code: "QUI", name: "Quirino", region: "II" },
        { code: "RIZ", name: "Rizal", region: "IV-A" },
        { code: "ROM", name: "Romblon", region: "IV-B" },
        { code: "SAM", name: "Samar", region: "VIII" },
        { code: "SAR", name: "Sarangani", region: "XII" },
        { code: "SIQ", name: "Siquijor", region: "VII" },
        { code: "SOR", name: "Sorsogon", region: "V" },
        { code: "SCO", name: "South Cotabato", region: "XII" },
        { code: "SLE", name: "Southern Leyte", region: "VIII" },
        { code: "SUK", name: "Sultan Kudarat", region: "XII" },
        { code: "SLU", name: "Sulu", region: "BARMM" },
        { code: "SUN", name: "Surigao del Norte", region: "XIII" },
        { code: "SUS", name: "Surigao del Sur", region: "XIII" },
        { code: "TAR", name: "Tarlac", region: "III" },
        { code: "TAW", name: "Tawi-Tawi", region: "BARMM" },
        { code: "ZMB", name: "Zambales", region: "III" },
        { code: "ZAN", name: "Zamboanga del Norte", region: "IX" },
        { code: "ZAS", name: "Zamboanga del Sur", region: "IX" },
        { code: "ZSI", name: "Zamboanga Sibugay", region: "IX" },
        { code: "DAV", name: "Davao del Norte", region: "XI" },
        { code: "DVO", name: "Davao Occidental", region: "XI" }
    ],
    cities: {
        "NCR": [
            "Caloocan", "Las Piñas", "Makati", "Malabon", "Mandaluyong",
            "Manila", "Marikina", "Muntinlupa", "Navotas", "Parañaque",
            "Pasay", "Pasig", "Pateros", "Quezon City", "San Juan",
            "Taguig", "Valenzuela"
        ],
        "ABR": ["Bangued", "Boliney", "Bucay", "Bucloc", "Daguioman", "Danglas", "Dolores", "La Paz", "Lacub", "Lagangilang", "Lagayan", "Langiden", "Licuan-Baay", "Luba", "Malibcong", "Manabo", "Peñarrubia", "Pidigan", "Pilar", "Sallapadan", "San Isidro", "San Juan", "San Quintin", "Tayum", "Tineg", "Tubo", "Villaviciosa"],
        "AGN": ["Butuan City", "Cabadbaran City", "Buenavista", "Carmen", "Jabonga", "Kitcharao", "Las Nieves", "Magallanes", "Nasipit", "Remedios T. Romualdez", "Santiago", "Tubay"],
        "AGS": ["Bayugan City", "Bunawan", "Esperanza", "La Paz", "Loreto", "Prosperidad", "Rosario", "San Francisco", "San Luis", "Santa Josefa", "Sibagat", "Talacogon", "Trento", "Veruela"],
        "AKL": ["Altavas", "Balete", "Banga", "Batan", "Buruanga", "Ibajay", "Kalibo", "Lezo", "Libacao", "Madalag", "Makato", "Malay", "Malinao", "Nabas", "New Washington", "Numancia", "Tangalan"],
        "ALB": ["Legazpi City", "Ligao City", "Tabaco City", "Bacacay", "Camalig", "Daraga", "Guinobatan", "Jovellar", "Libon", "Malilipot", "Malinao", "Manito", "Oas", "Pio Duran", "Polangui", "Rapu-Rapu", "Santo Domingo", "Tiwi"],
        "ANT": ["San Jose de Buenavista", "Anini-y", "Barbaza", "Belison", "Bugasong", "Caluya", "Culasi", "Hamtic", "Laua-an", "Libertad", "Pandan", "Patnongon", "San Remigio", "Sebaste", "Sibalom", "Tibiao", "Tobias Fornier", "Valderrama"],
        "APA": ["Calanasan", "Conner", "Flora", "Kabugao", "Luna", "Pudtol", "Santa Marcela"],
        "AUR": ["Baler", "Casiguran", "Dilasag", "Dinalungan", "Dingalan", "Dipaculao", "Maria Aurora", "San Luis"],
        "BAN": ["Balanga City", "Abucay", "Bagac", "Dinalupihan", "Hermosa", "Limay", "Mariveles", "Morong", "Orani", "Orion", "Pilar", "Samal"],
        "BTN": ["Basco", "Itbayat", "Ivana", "Mahatao", "Sabtang", "Uyugan"],
        "BTG": ["Batangas City", "Lipa City", "Tanauan City", "Santo Tomas City", "Agoncillo", "Alitagtag", "Balayan", "Balete", "Bauan", "Calaca", "Calatagan", "Cuenca", "Ibaan", "Laurel", "Lemery", "Lian", "Lobo", "Mabini", "Malvar", "Mataasnakahoy", "Nasugbu", "Padre Garcia", "Rosario", "San Jose", "San Juan", "San Luis", "San Nicolas", "San Pascual", "Santa Teresita", "Taal", "Talisay", "Taysan", "Tingloy", "Tuy"],
        "BEN": ["Baguio City", "Atok", "Bakun", "Bokod", "Buguias", "Itogon", "Kabayan", "Kapangan", "Kibungan", "La Trinidad", "Mankayan", "Sablan", "Tuba", "Tublay"],
        "BIL": ["Almeria", "Biliran", "Cabucgayan", "Caibiran", "Culaba", "Kawayan", "Maripipi", "Naval"],
        "BOH": ["Tagbilaran City", "Alburquerque", "Alicia", "Anda", "Antequera", "Baclayon", "Balilihan", "Batuan", "Bilar", "Buenavista", "Calape", "Candijay", "Carmen", "Catigbian", "Clarin", "Corella", "Cortes", "Dagohoy", "Danao", "Dauis", "Dimiao", "Duero", "Garcia Hernandez", "Guindulman", "Inabanga", "Jagna", "Jetafe", "Lila", "Loay", "Loboc", "Loon", "Mabini", "Maribojoc", "Panglao", "Pilar", "President Carlos P. Garcia", "Sagbayan", "San Isidro", "San Miguel", "Sevilla", "Sierra Bullones", "Sikatuna", "Talibon", "Trinidad", "Tubigon", "Ubay", "Valencia"],
        "BUK": ["Malaybalay City", "Valencia City", "Baungon", "Cabanglasan", "Damulog", "Dangcagan", "Don Carlos", "Impasugong", "Kadingilan", "Kalilangan", "Kibawe", "Kitaotao", "Lantapan", "Libona", "Malitbog", "Manolo Fortich", "Maramag", "Pangantucan", "Quezon", "San Fernando", "Sumilao", "Talakag"],
        "BUL": ["Malolos City", "Meycauayan City", "San Jose del Monte City", "Angat", "Balagtas", "Baliuag", "Bocaue", "Bulacan", "Bustos", "Calumpit", "Doña Remedios Trinidad", "Guiguinto", "Hagonoy", "Marilao", "Norzagaray", "Obando", "Pandi", "Paombong", "Plaridel", "Pulilan", "San Ildefonso", "San Miguel", "San Rafael", "Santa Maria"],
        "CAG": ["Tuguegarao City", "Abulug", "Alcala", "Allacapan", "Amulung", "Aparri", "Baggao", "Ballesteros", "Buguey", "Calayan", "Camalaniugan", "Claveria", "Enrile", "Gattaran", "Gonzaga", "Iguig", "Lal-lo", "Lasam", "Pamplona", "Peñablanca", "Piat", "Rizal", "Sanchez-Mira", "Santa Ana", "Santa Praxedes", "Santa Teresita", "Santo Niño", "Solana", "Tuao"],
        "CAN": ["Daet", "Basud", "Capalonga", "Jose Panganiban", "Labo", "Mercedes", "Paracale", "San Lorenzo Ruiz", "San Vicente", "Santa Elena", "Talisay", "Vinzons"],
        "CAS": ["Naga City", "Iriga City", "Baao", "Balatan", "Bato", "Bombon", "Buhi", "Bula", "Cabusao", "Calabanga", "Camaligan", "Canaman", "Caramoan", "Del Gallego", "Gainza", "Garchitorena", "Goa", "Lagonoy", "Libmanan", "Lupi", "Magarao", "Milaor", "Minalabac", "Nabua", "Ocampo", "Pamplona", "Pasacao", "Pili", "Presentacion", "Ragay", "Sagñay", "San Fernando", "San Jose", "Sipocot", "Siruma", "Tigaon", "Tinambac"],
        "CAM": ["Mambajao", "Catarman", "Guinsiliban", "Mahinog", "Sagay"],
        "CAP": ["Roxas City", "Cuartero", "Dao", "Dumalag", "Dumarao", "Ivisan", "Jamindan", "Ma-ayon", "Mambusao", "Panay", "Panitan", "Pilar", "Pontevedra", "President Roxas", "Sapi-an", "Sigma", "Tapaz"],
        "CAT": ["Virac", "Bagamanoc", "Baras", "Bato", "Caramoran", "Gigmoto", "Pandan", "Panganiban", "San Andres", "San Miguel", "Viga"],
        "CAV": ["Bacoor City", "Cavite City", "Dasmariñas City", "General Trias City", "Imus City", "Tagaytay City", "Trece Martires City", "Alfonso", "Amadeo", "Carmona", "Gen. Mariano Alvarez", "Indang", "Kawit", "Magallanes", "Maragondon", "Mendez", "Naic", "Noveleta", "Rosario", "Silang", "Tanza", "Ternate"],
        "CEB": ["Cebu City", "Mandaue City", "Lapu-Lapu City", "Talisay City", "Danao City", "Toledo City", "Naga City", "Carcar City", "Bogo City", "Alcantara", "Alcoy", "Alegria", "Aloguinsan", "Argao", "Asturias", "Badian", "Balamban", "Bantayan", "Barili", "Boljoon", "Borbon", "Carmen", "Catmon", "Compostela", "Consolacion", "Cordova", "Daanbantayan", "Dalaguete", "Dumanjug", "Ginatilan", "Liloan", "Madridejos", "Malabuyoc", "Medellin", "Minglanilla", "Moalboal", "Oslob", "Pilar", "Pinamungajan", "Poro", "Ronda", "Samboan", "San Fernando", "San Francisco", "San Remigio", "Santa Fe", "Santander", "Sibonga", "Sogod", "Tabogon", "Tabuelan", "Tuburan", "Tudela"],
        "COM": ["Kidapawan City", "Alamada", "Aleosan", "Antipas", "Arakan", "Banisilan", "Carmen", "Kabacan", "Libungan", "Magpet", "Makilala", "Matalam", "Midsayap", "M'lang", "Pigcawayan", "Pikit", "President Roxas", "Tulunan"],
        "DAV": ["Tagum City", "Panabo City", "Island Garden City of Samal", "Asuncion", "Braulio E. Dujali", "Carmen", "Kapalong", "New Corella", "San Isidro", "Santo Tomas", "Talaingod"],
        "DAS": ["Davao City", "Digos City", "Bansalan", "Don Marcelino", "Hagonoy", "Jose Abad Santos", "Kiblawan", "Magsaysay", "Malalag", "Malita", "Matanao", "Padada", "Santa Cruz", "Santa Maria", "Sarangani", "Sulop"],
        "DAO": ["Mati City", "Baganga", "Banaybanay", "Boston", "Caraga", "Cateel", "Governor Generoso", "Lupon", "Manay", "San Isidro", "Tarragona"],
        "DIN": ["Basilisa", "Cagdianao", "Dinagat", "Libjo", "Loreto", "San Jose", "Tubajon"],
        "EAS": ["Borongan City", "Arteche", "Balangiga", "Balangkayan", "Can-avid", "Dolores", "General MacArthur", "Giporlos", "Guiuan", "Hernani", "Jipapad", "Lawaan", "Llorente", "Maslog", "Maydolong", "Mercedes", "Oras", "Quinapondan", "Salcedo", "San Julian", "San Policarpo", "Sulat", "Taft"],
        "GUI": ["Jordan", "Buenavista", "Nueva Valencia", "San Lorenzo", "Sibunag"],
        "IFU": ["Lagawe", "Aguinaldo", "Alfonso Lista", "Asipulo", "Banaue", "Hingyon", "Hungduan", "Kiangan", "Lamut", "Mayoyao", "Tinoc"],
        "ILN": ["Laoag City", "Batac City", "Adams", "Bacarra", "Badoc", "Bangui", "Banna", "Burgos", "Carasi", "Currimao", "Dingras", "Dumalneg", "Marcos", "Nueva Era", "Pagudpud", "Paoay", "Pasuquin", "Piddig", "Pinili", "San Nicolas", "Sarrat", "Solsona", "Vintar"],
        "ILS": ["Vigan City", "Candon City", "Alilem", "Banayoyo", "Bantay", "Burgos", "Cabugao", "Caoayan", "Cervantes", "Galimuyod", "Gregorio del Pilar", "Lidlidda", "Magsingal", "Nagbukel", "Narvacan", "Quirino", "Salcedo", "San Emilio", "San Esteban", "San Ildefonso", "San Juan", "San Vicente", "Santa", "Santa Catalina", "Santa Cruz", "Santa Lucia", "Santa Maria", "Santiago", "Santo Domingo", "Sigay", "Sinait", "Sugpon", "Suyo", "Tagudin"],
        "ILO": ["Iloilo City", "Passi City", "Ajuy", "Alimodian", "Anilao", "Badiangan", "Balasan", "Banate", "Barotac Nuevo", "Barotac Viejo", "Batad", "Bingawan", "Cabatuan", "Calinog", "Carles", "Concepcion", "Dingle", "Dueñas", "Dumangas", "Estancia", "Guimbal", "Igbaras", "Janiuay", "Lambunao", "Leganes", "Lemery", "Leon", "Maasin", "Miagao", "Mina", "New Lucena", "Oton", "Pavia", "Pototan", "San Dionisio", "San Enrique", "San Joaquin", "San Miguel", "San Rafael", "Santa Barbara", "Sara", "Tigbauan", "Tubungan", "Zarraga"],
        "ISA": ["Ilagan City", "Cauayan City", "Santiago City", "Alicia", "Angadanan", "Aurora", "Benito Soliven", "Burgos", "Cabagan", "Cabatuan", "Cordon", "Delfin Albano", "Dinapigue", "Divilacan", "Echague", "Gamu", "Jones", "Luna", "Maconacon", "Mallig", "Naguilian", "Palanan", "Quezon", "Quirino", "Ramon", "Reina Mercedes", "Roxas", "San Agustin", "San Guillermo", "San Isidro", "San Manuel", "San Mariano", "San Mateo", "San Pablo", "Santa Maria", "Santo Tomas", "Tumauini"],
        "KAL": ["Tabuk City", "Balbalan", "Lubuagan", "Pasil", "Pinukpuk", "Rizal", "Tanudan", "Tinglayan"],
        "LAG": ["Santa Rosa City", "Calamba City", "San Pablo City", "Biñan City", "Cabuyao City", "San Pedro City", "Alaminos", "Bay", "Calauan", "Cavinti", "Famy", "Kalayaan", "Liliw", "Los Baños", "Luisiana", "Lumban", "Mabitac", "Magdalena", "Majayjay", "Nagcarlan", "Paete", "Pagsanjan", "Pakil", "Pangil", "Pila", "Rizal", "Santa Cruz", "Santa Maria", "Siniloan", "Victoria"],
        "LAN": ["Iligan City", "Bacolod", "Baloi", "Baroy", "Kapatagan", "Kauswagan", "Kolambugan", "Lala", "Linamon", "Magsaysay", "Maigo", "Matungao", "Munai", "Nunungan", "Pantao Ragat", "Pantar", "Poona Piagapo", "Salvador", "Sapad", "Sultan Naga Dimaporo", "Tagoloan", "Tangcal", "Tubod"],
        "LAS": ["Marawi City", "Bacolod-Kalawi", "Balabagan", "Balindong", "Bayang", "Binidayan", "Buadiposo-Buntong", "Bubong", "Butig", "Calanogas", "Ditsaan-Ramain", "Ganassi", "Kapai", "Kapatagan", "Lumba-Bayabao", "Lumbaca-Unayan", "Lumbatan", "Lumbayanague", "Madalum", "Madamba", "Maguing", "Malabang", "Marantao", "Marogong", "Masiu", "Mulondo", "Pagayawan", "Piagapo", "Picong", "Pualas", "Saguiaran", "Sultan Dumalondong", "Tagoloan II", "Tamparan", "Taraka", "Tubaran", "Tugaya", "Wao"],
        "LUN": ["San Fernando City", "Agoo", "Aringay", "Bacnotan", "Bagulin", "Balaoan", "Bangar", "Bauang", "Burgos", "Caba", "Luna", "Naguilian", "Pugo", "Rosario", "San Gabriel", "San Juan", "Santo Tomas", "Santol", "Sudipen", "Tubao"],
        "LEY": ["Tacloban City", "Ormoc City", "Abuyog", "Alangalang", "Albuera", "Babatngon", "Barugo", "Bato", "Baybay City", "Burauen", "Calubian", "Capoocan", "Carigara", "Dagami", "Dulag", "Hilongos", "Hindang", "Inopacan", "Isabel", "Jaro", "Javier", "Julita", "Kananga", "La Paz", "Leyte", "MacArthur", "Mahaplag", "Matag-ob", "Matalom", "Mayorga", "Merida", "Palo", "Palompon", "Pastrana", "San Isidro", "San Miguel", "Santa Fe", "Tabango", "Tabontabon", "Tanauan", "Tolosa", "Tunga", "Villaba"],
        "MAG": ["Cotabato City", "Ampatuan", "Barira", "Buldon", "Buluan", "Datu Abdullah Sangki", "Datu Anggal Midtimbang", "Datu Blah T. Sinsuat", "Datu Hoffer Ampatuan", "Datu Montawal", "Datu Odin Sinsuat", "Datu Paglas", "Datu Piang", "Datu Salibo", "Datu Saudi-Ampatuan", "Datu Unsay", "Gen. S. K. Pendatun", "Guindulungan", "Kabuntalan", "Mamasapano", "Mangudadatu", "Matanog", "Northern Kabuntalan", "Pagalungan", "Paglat", "Pandag", "Parang", "Rajah Buayan", "Shariff Aguak", "Shariff Saydona Mustapha", "South Upi", "Sultan Kudarat", "Sultan Mastura", "Sultan sa Barongis", "Talayan", "Talitay", "Upi"],
        "MRN": ["Boac", "Buenavista", "Gasan", "Mogpog", "Santa Cruz", "Torrijos"],
        "MSC": ["Masbate City", "Aroroy", "Baleno", "Balud", "Batuan", "Cataingan", "Cawayan", "Claveria", "Dimasalang", "Esperanza", "Mandaon", "Milagros", "Mobo", "Monreal", "Palanas", "Pio V. Corpuz", "Placer", "San Fernando", "San Jacinto", "San Pascual", "Uson"],
        "MSR": ["Oroquieta City", "Ozamiz City", "Tangub City", "Aloran", "Baliangao", "Bonifacio", "Calamba", "Clarin", "Concepcion", "Don Victoriano Chiongbian", "Jimenez", "Lopez Jaena", "Panaon", "Plaridel", "Sapang Dalaga", "Sinacaban", "Tudela"],
        "MSO": ["Cagayan de Oro City", "Gingoog City", "El Salvador City", "Alubijid", "Balingasag", "Balingoan", "Binuangan", "Claveria", "Gitagum", "Initao", "Jasaan", "Kinoguitan", "Lagonglong", "Laguindingan", "Libertad", "Lugait", "Magsaysay", "Manticao", "Medina", "Naawan", "Opol", "Salay", "Sugbongcogon", "Tagoloan", "Talisayan", "Villanueva"],
        "MOP": ["Bontoc", "Barlig", "Bauko", "Besao", "Natonin", "Paracelis", "Sabangan", "Sadanga", "Sagada", "Tadian"],
        "NEC": ["Bacolod City", "Bago City", "Cadiz City", "Escalante City", "Himamaylan City", "Kabankalan City", "La Carlota City", "Sagay City", "San Carlos City", "Silay City", "Sipalay City", "Talisay City", "Victorias City", "Binalbagan", "Calatrava", "Candoni", "Cauayan", "Enrique B. Magalona", "Hinigaran", "Hinoba-an", "Ilog", "Isabela", "La Castellana", "Manapla", "Moises Padilla", "Murcia", "Pontevedra", "Pulupandan", "Salvador Benedicto", "San Enrique", "Toboso", "Valladolid"],
        "NER": ["Dumaguete City", "Bayawan City", "Bais City", "Canlaon City", "Tanjay City", "Guihulngan City", "Amlan", "Ayungon", "Bacong", "Basay", "Bindoy", "Dauin", "Jimalalud", "La Libertad", "Mabinay", "Manjuyod", "Pamplona", "San Jose", "Santa Catalina", "Siaton", "Sibulan", "Tayasan", "Valencia", "Vallehermoso", "Zamboanguita"],
        "NSA": ["Catarman", "Allen", "Biri", "Bobon", "Capul", "Catubig", "Gamay", "Laoang", "Lapinig", "Las Navas", "Lavezares", "Lope de Vega", "Mapanas", "Mondragon", "Palapag", "Pambujan", "Rosario", "San Antonio", "San Isidro", "San Jose", "San Roque", "San Vicente", "Silvino Lobos", "Victoria"],
        "NUE": ["Cabanatuan City", "Gapan City", "San Jose City", "Palayan City", "Science City of Muñoz", "Aliaga", "Bongabon", "Cabiao", "Carranglan", "Cuyapo", "Gabaldon", "Gen. Mamerto Natividad", "Gen. Tinio", "Guimba", "Jaen", "Laur", "Licab", "Llanera", "Lupao", "Nampicuan", "Pantabangan", "Peñaranda", "Quezon", "Rizal", "San Antonio", "San Isidro", "San Leonardo", "Santa Rosa", "Santo Domingo", "Talavera", "Talugtug", "Zaragoza"],
        "NUV": ["Bayombong", "Ambaguio", "Aritao", "Bagabag", "Bambang", "Diadi", "Dupax del Norte", "Dupax del Sur", "Kasibu", "Kayapa", "Quezon", "Santa Fe", "Solano", "Villaverde"],
        "MDC": ["Mamburao", "Abra de Ilog", "Calintaan", "Looc", "Lubang", "Magsaysay", "Paluan", "Rizal", "Sablayan", "San Jose", "Santa Cruz"],
        "MDR": ["Calapan City", "Baco", "Bansud", "Bongabong", "Bulalacao", "Gloria", "Mansalay", "Naujan", "Pinamalayan", "Pola", "Puerto Galera", "Roxas", "San Teodoro", "Socorro", "Victoria"],
        "PLW": ["Puerto Princesa City", "Aborlan", "Agutaya", "Araceli", "Balabac", "Bataraza", "Brooke's Point", "Busuanga", "Cagayancillo", "Coron", "Culion", "Cuyo", "Dumaran", "El Nido", "Kalayaan", "Linapacan", "Magsaysay", "Narra", "Quezon", "Rizal", "Roxas", "San Vicente", "Sofronio Española", "Taytay"],
        "PAM": ["San Fernando City", "Angeles City", "Mabalacat City", "Apalit", "Arayat", "Bacolor", "Candaba", "Floridablanca", "Guagua", "Lubao", "Macabebe", "Magalang", "Masantol", "Mexico", "Minalin", "Porac", "San Luis", "San Simon", "Santa Ana", "Santa Rita", "Santo Tomas", "Sasmuan"],
        "PAN": ["Dagupan City", "San Carlos City", "Urdaneta City", "Alaminos City", "Agno", "Aguilar", "Alcala", "Anda", "Asingan", "Balungao", "Bani", "Basista", "Bautista", "Bayambang", "Binalonan", "Binmaley", "Bolinao", "Bugallon", "Burgos", "Calasiao", "Dasol", "Infanta", "Labrador", "Laoac", "Lingayen", "Mabini", "Malasiqui", "Manaoag", "Mangaldan", "Mangatarem", "Mapandan", "Natividad", "Pozorrubio", "Rosales", "San Fabian", "San Jacinto", "San Manuel", "San Nicolas", "San Quintin", "Santa Barbara", "Santa Maria", "Santo Tomas", "Sison", "Sual", "Tayug", "Umingan", "Urbiztondo", "Villasis"],
        "QUE": ["Lucena City", "Tayabas City", "Agdangan", "Alabat", "Atimonan", "Buenavista", "Burdeos", "Calauag", "Candelaria", "Catanauan", "Dolores", "General Luna", "General Nakar", "Guinayangan", "Gumaca", "Infanta", "Jomalig", "Lopez", "Lucban", "Macalelon", "Mauban", "Mulanay", "Padre Burgos", "Pagbilao", "Panukulan", "Patnanungan", "Perez", "Pitogo", "Plaridel", "Polillo", "Quezon", "Real", "Sampaloc", "San Andres", "San Antonio", "San Francisco", "San Narciso", "Sariaya", "Tagkawayan", "Tiaong", "Unisan"],
        "QUI": ["Cabarroguis", "Aglipay", "Diffun", "Maddela", "Nagtipunan", "Saguday"],
        "RIZ": ["Antipolo City", "Angono", "Baras", "Binangonan", "Cainta", "Cardona", "Jalajala", "Morong", "Pililla", "Rodriguez", "San Mateo", "Tanay", "Taytay", "Teresa"],
        "ROM": ["Romblon", "Alcantara", "Banton", "Cajidiocan", "Calatrava", "Concepcion", "Corcuera", "Ferrol", "Looc", "Magdiwang", "Odiongan", "San Agustin", "San Andres", "San Fernando", "San Jose", "Santa Fe", "Santa Maria"],
        "SAM": ["Catbalogan City", "Calbayog City", "Almagro", "Basey", "Calbayog", "Daram", "Gandara", "Hinabangan", "Jiabong", "Marabut", "Matuguinao", "Motiong", "Pagsanghan", "Paranas", "Pinabacdao", "San Jorge", "San Jose de Buan", "San Sebastian", "Santa Margarita", "Santa Rita", "Santo Niño", "Tagapul-an", "Talalora", "Tarangnan", "Villareal", "Zumarraga"],
        "SAR": ["Alabel", "Glan", "Kiamba", "Maasim", "Maitum", "Malapatan", "Malungon"],
        "SIQ": ["Siquijor", "Enrique Villanueva", "Larena", "Lazi", "Maria", "San Juan"],
        "SOR": ["Sorsogon City", "Barcelona", "Bulan", "Bulusan", "Casiguran", "Castilla", "Donsol", "Gubat", "Irosin", "Juban", "Magallanes", "Matnog", "Pilar", "Prieto Diaz", "Santa Magdalena"],
        "SCO": ["Koronadal City", "General Santos City", "Banga", "Lake Sebu", "Norala", "Polomolok", "Santo Niño", "Surallah", "T'Boli", "Tampakan", "Tantangan", "Tupi"],
        "SLE": ["Maasin City", "Anahawan", "Bontoc", "Hinunangan", "Hinundayan", "Libagon", "Liloan", "Limasawa", "Macrohon", "Malitbog", "Padre Burgos", "Pintuyan", "Saint Bernard", "San Francisco", "San Juan", "San Ricardo", "Silago", "Sogod", "Tomas Oppus"],
        "SUK": ["Tacurong City", "Bagumbayan", "Columbio", "Esperanza", "Isulan", "Kalamansig", "Lambayong", "Lebak", "Lutayan", "Palimbang", "President Quirino", "Senator Ninoy Aquino"],
        "SLU": ["Jolo", "Hadji Panglima Tahil", "Indanan", "Kalingalan Caluang", "Lugus", "Luuk", "Maimbung", "Old Panamao", "Omar", "Pandami", "Panglima Estino", "Pangutaran", "Parang", "Pata", "Patikul", "Siasi", "Talipao", "Tapul"],
        "SUN": ["Surigao City", "Alegria", "Bacuag", "Burgos", "Claver", "Dapa", "Del Carmen", "General Luna", "Gigaquit", "Mainit", "Malimono", "Pilar", "Placer", "San Benito", "San Francisco", "San Isidro", "Santa Monica", "Sison", "Socorro", "Tagana-an", "Tubod"],
        "SUS": ["Bislig City", "Tandag City", "Barobo", "Bayabas", "Cagwait", "Cantilan", "Carmen", "Carrascal", "Cortes", "Hinatuan", "Lanuza", "Lianga", "Lingig", "Madrid", "Marihatag", "San Agustin", "San Miguel", "Tagbina", "Tago"],
        "TAR": ["Tarlac City", "Anao", "Bamban", "Camiling", "Capas", "Concepcion", "Gerona", "La Paz", "Mayantoc", "Moncada", "Paniqui", "Pura", "Ramos", "San Clemente", "San Jose", "San Manuel", "Santa Ignacia", "Victoria"],
        "TAW": ["Bongao", "Languyan", "Mapun", "Panglima Sugala", "Sapa-Sapa", "Sibutu", "Simunul", "Sitangkai", "South Ubian", "Tandubas", "Turtle Islands"],
        "ZMB": ["Olongapo City", "Botolan", "Cabangan", "Candelaria", "Castillejos", "Iba", "Masinloc", "Palauig", "San Antonio", "San Felipe", "San Marcelino", "San Narciso", "Santa Cruz", "Subic"],
        "ZAN": ["Dipolog City", "Dapitan City", "Bacungan", "Baliguian", "Godod", "Gutalac", "Jose Dalman", "Kalawit", "Katipunan", "La Libertad", "Labason", "Leon B. Postigo", "Liloy", "Manukan", "Mutia", "Piñan", "Polanco", "Pres. Manuel A. Roxas", "Rizal", "Salug", "Sergio Osmeña Sr.", "Siayan", "Sibuco", "Sibutad", "Sindangan", "Siocon", "Sirawai", "Tampilisan"],
        "ZAS": ["Pagadian City", "Zamboanga City", "Aurora", "Bayog", "Dimataling", "Dinas", "Dumalinao", "Dumingag", "Guipos", "Josefina", "Kumalarang", "Labangan", "Lakewood", "Lapuyan", "Mahayag", "Margosatubig", "Midsalip", "Molave", "Pitogo", "Ramon Magsaysay", "San Miguel", "San Pablo", "Sominot", "Tabina", "Tambulig", "Tigbao", "Tukuran", "Vincenzo A. Sagun"],
        "ZSI": ["Ipil", "Alicia", "Buug", "Diplahan", "Imelda", "Kabasalan", "Mabuhay", "Malangas", "Naga", "Olutanga", "Payao", "Roseller Lim", "Siay", "Talusan", "Titay", "Tungawan"],
        "DVO": ["Jose Abad Santos", "Malita", "Santa Maria", "Sarangani", "Don Marcelino"]
    },
    // Barangays by Province -> City
    barangays: {
        // Davao del Sur Barangays
        "DAS": {
            "Davao City": ["Acacia", "Agdao", "Alambre", "Alejandra Navarro (Lasang)", "Alfonso Angliongto Sr.", "Angalan", "Atan-Awe", "Baganihan", "Bago Aplaya", "Bago Gallera", "Bago Oshiro", "Baguio", "Balengaeng", "Baliok", "Bangkas Heights", "Bantol", "Baracatan", "Barangay 1-A", "Barangay 2-A", "Barangay 3-A", "Barangay 4-A", "Barangay 5-A", "Barangay 6-A", "Barangay 7-A", "Barangay 8-A", "Barangay 9-A", "Barangay 10-A", "Barangay 11-B", "Barangay 12-B", "Barangay 13-B", "Barangay 14-B", "Barangay 15-B", "Barangay 16-B", "Barangay 17-B", "Barangay 18-B", "Barangay 19-B", "Barangay 20-B", "Barangay 21-C", "Barangay 22-C", "Barangay 23-C", "Barangay 24-C", "Barangay 25-C", "Barangay 26-C", "Barangay 27-C", "Barangay 28-C", "Barangay 29-C", "Barangay 30-C", "Barangay 31-D", "Barangay 32-D", "Barangay 33-D", "Barangay 34-D", "Barangay 35-D", "Barangay 36-D", "Barangay 37-D", "Barangay 38-D", "Barangay 39-D", "Barangay 40-D", "Bato", "Bayabas", "Biao Escuela", "Biao Guianga", "Biao Joaquin", "Binugao", "Bucana", "Buda", "Buhangin", "Bunawan", "Cabantian", "Cadalian", "Calinan", "Callawa", "Camansi", "Carmen", "Catalunan Grande", "Catalunan Pequeño", "Catigan", "Cawayan", "Centro (San Juan)", "Colosas", "Communal", "Crossing Bayabas", "Dacudao", "Dalag", "Dalagdag", "Daliao", "Daliaon Plantation", "Datu Salumay", "Dominga", "Dumoy", "Eden", "Fatima (Benowang)", "Gatungan", "Gov. Paciano Bangoy", "Gov. Vicente Duterte", "Gumalang", "Gumitan", "Ilang", "Inayangan", "Indangan", "Kap. Tomas Monteverde Sr.", "Kilate", "Lacson", "Lamanan", "Lampianao", "Langub", "Lapu-lapu", "Leon Garcia Sr.", "Lizada", "Los Amigos", "Lubogan", "Lumiad", "Ma-a", "Mabuhay", "Magsaysay", "Magtuod", "Mahayag", "Malabog", "Malagos", "Malamba", "Manambulan", "Mandug", "Manuel Guianga", "Mapula", "Marapangi", "Marilog", "Matina Aplaya", "Matina Biao", "Matina Crossing", "Matina Pangi", "Megkawayan", "Mintal", "Mudiang", "Mulig", "New Carmen", "New Valencia", "Pampanga", "Panacan", "Panalum", "Pandaitan", "Pangyan", "Paquibato", "Paradise Embak", "Rafael Castillo", "Riverside", "Salapawan", "Salaysay", "Saloy", "San Antonio", "San Isidro (Licanan)", "Santo Niño", "Sasa", "Sibulan", "Sirawan", "Sirib", "Suawan (Tuli)", "Subasta", "Sumimao", "Tacunan", "Tagakpan", "Tagluno", "Tagurano", "Talandang", "Talomo", "Talomo River", "Tamayong", "Tambobong", "Tamugan", "Tapak", "Tawan-Tawan", "Tibuloy", "Tibungco", "Tigatto", "Toril", "Tugbok", "Tungakalan", "Ubalde", "Ula", "Vicente Hizon Sr.", "Waan", "Wangan", "Wilfredo Aquino", "Wines"],
            "Digos City": ["Aplaya", "Balabag", "Binaton", "Cogon", "Colorado", "Dawis", "Dulangan", "Goma", "Igpit", "Kapatagan", "Kiagot", "Lungag", "Matti", "Ruparan", "San Agustin", "San Jose", "San Miguel", "San Roque", "Sinawilan", "Soong", "Tiguman", "Tres de Mayo", "Zone 1 (Poblacion)", "Zone 2 (Poblacion)", "Zone 3 (Poblacion)"],
            "Bansalan": ["Alegre", "Alta Vista", "Anonang", "Bitaug", "Bonifacio", "Buenavista", "Darapuay", "Dolo", "Eman", "Kinuskusan", "Libertad", "Linawan", "Mabuhay", "Magsaysay", "Managa", "New Clarin", "Poblacion Uno", "Poblacion Dos", "Rizal", "Sibayan", "Sinayawan", "Tubod"],
            "Don Marcelino": ["Baluntaya", "Calian", "Dalupan", "Kinanga", "Lanao", "Lapuan", "Lawa", "Linadasan", "Mabuhay", "North Lamidan", "Nueva Villa", "Poblacion", "South Lamidan", "Talagutong", "Talaguton", "Fishing Village"],
            "Hagonoy": ["Aplaya", "Balutakay", "Calasuyan", "Guihing", "Hagonoy Crossing", "Kibuaya", "La Union", "Lanuro", "Lapulabao", "Leling", "Mahayahay", "Malabang", "Maliit Digos", "New Quezon", "Paligue", "Poblacion", "Sacub", "San Guillermo", "San Isidro", "Sinawilan", "Tologan"],
            "Jose Abad Santos": ["Amas", "Bawang", "Caburan Big", "Caburan Small", "Camalian", "Cogon", "Coronobe", "Guihing", "Isis", "Katipunan", "La Union", "Lanao", "Lapuan", "Linadasan", "Malalag", "Mangile", "Melon", "Ngan", "Oton", "Poblacion", "Saliducon", "Sugal", "Tabayon", "Tanuman"],
            "Kiblawan": ["Bagong Negros", "Bagumbayan", "Balasiao", "Bunot", "Cogon-Bacaca", "Dapok", "Ihan", "Kibongbong", "Kimlawis", "Kisulan", "Lati-an", "Manual", "Maraga-a", "Molopolo", "New Sibonga", "Panaglib", "Pasig", "Poblacion", "Pob. East", "Pob. West", "Sinapulan", "Tacub", "Tacul", "Waterfall"],
            "Magsaysay": ["Bacungan", "Balnate", "Barayong", "Dalawinon", "Dalumay", "Dolo", "Igdaan", "Kanapulo", "Kusan", "Lower Bala", "Mabini", "Malawanit", "New Ilocos", "New Opon", "Poblacion", "San Isidro", "San Miguel", "Tacul", "Upper Bala"],
            "Malalag": ["Bagumbayan", "Bolton", "Caputian", "Ibo", "Kiblagon", "Lapla", "Mabini", "New Baclayon", "Pitu", "Poblacion", "San Isidro", "Tagansule"],
            "Malita": ["Bito", "Bolila", "Buhangin", "Culaman", "Fishing Village", "Kibalatong", "Kidalapong", "Lasang", "Little Baguio", "Mana", "Manuel Peralta", "Poblacion", "Pangaleon", "Pinalpalan", "Sangay", "Tagdaliri", "Tubalan"],
            "Matanao": ["Asbang", "Asinan", "Bagumbayan", "Bangkal", "Buas", "Camanchiles", "Colonsabak", "Daluman", "Dunggoan", "Kalaong", "Kanapulo", "Katipunan", "La Suerte", "Lapu-lapu", "Lower Marber", "Mabuhay", "Maibo", "Manga", "New Katipunan", "New Visayas", "Poblacion", "Saboy", "San Jose", "San Miguel", "San Vicente", "Savoy", "Sinawilan", "Tamlangon", "Towak", "Upper Marber"],
            "Padada": ["Almendras", "Harada Butai", "Lower Limonzo", "Lower Malinao", "Northern Paligue", "Palili", "Piape", "Poblacion", "Punta Piape", "San Isidro", "Southern Paligue", "Upper Limonzo", "Upper Malinao"],
            "Santa Cruz": ["Astorga", "Bato", "Coronon", "Darong", "Jose Rizal", "Matutungan", "Melilia", "Saliducon", "Sibulan", "Sinoron", "Tagabuli", "Tibolo", "Zone 1 (Poblacion)", "Zone 2 (Poblacion)", "Zone 3 (Poblacion)", "Zone 4 (Poblacion)"],
            "Santa Maria": ["Basiawan", "Buca", "Cadaatan", "Kidadan", "Kisante", "Malalag Tubig", "Ogpao", "Poblacion", "San Agustin", "San Isidro", "San Jose", "San Pedro", "Santo Rosario", "Tanglad"],
            "Sarangani": ["Batuganding", "Camahual", "Gomtago", "Laker", "Lipol", "Mabila", "Patong", "Poblacion", "Tinina"],
            "Sulop": ["Balasinon", "Buguis", "Carre", "Katipunan", "Labon", "Lawa", "Lapla", "Lumabat", "Malabang", "Parame", "Poblacion", "Sab-a", "Tabayon", "Tanwalang", "Waterfall"]
        },
        // Metro Manila Barangays (sample - NCR is commonly searched)
        "NCR": {
            "Manila": ["Balic-Balic", "Ermita", "Intramuros", "Malate", "Paco", "Pandacan", "Port Area", "Quiapo", "Sampaloc", "San Andres", "San Miguel", "San Nicolas", "Santa Ana", "Santa Cruz", "Santa Mesa", "Tondo"],
            "Quezon City": ["Alicia", "Apolonio Samson", "Aurora", "Baesa", "Bagong Lipunan ng Crame", "Bagong Pag-asa", "Bahay Toro", "Balingasa", "Batasan Hills", "Botocan", "Central", "Claro", "Commonwealth", "Culiat", "Damar", "Damayan", "Del Monte", "Diliman", "Dioquino Zobel", "Don Manuel", "Doña Aurora", "Doña Imelda", "Doña Josefa", "E. Rodriguez", "Escopa", "Fairview", "Greater Lagro", "Holy Spirit", "Horseshoe", "Kalusugan", "Kamuning", "Katipunan", "Kaunlaran", "Kristong Hari", "Krus na Ligas", "Laging Handa", "Libis", "Loyola Heights", "Maharlika", "Malaya", "Mangga", "Manresa", "Mariana", "Mariblo", "Marilag", "Masagana", "Matandang Balara", "Milagrosa", "N.S. Amoranto", "Nagkaisang Nayon", "Nayong Kanluran", "New Era", "North Fairview", "Novaliches", "Obrero", "Old Capitol Site", "Paang Bundok", "Pag-ibig sa Nayon", "Paligsahan", "Paltok", "Pansol", "Paraiso", "Pasong Putik Proper", "Pasong Tamo", "Payatas", "Phil-Am", "Pinagkaisahan", "Pinyahan", "Project 6", "Project 8", "Quezon Memorial Circle", "R. Magsaysay", "Ramon Magsaysay", "Roxas", "Sacred Heart", "Saint Ignatius", "Saint Peter", "Salvacion", "San Agustin", "San Antonio", "San Bartolome", "San Isidro", "San Jose", "San Martin de Porres", "San Roque", "San Vicente", "Santa Cruz", "Santa Lucia", "Santa Monica", "Santa Teresita", "Santo Cristo", "Santo Domingo", "Santo Niño", "Santol", "Sauyo", "Sienna", "Sikatuna Village", "Silangan", "Socorro", "South Triangle", "Tagumpay", "Talayan", "Talipapa", "Tandang Sora", "Tatalon", "Teachers Village East", "Teachers Village West", "Ugong Norte", "Unang Sigaw", "UP Campus", "UP Village", "Valencia", "Vasra", "Veterans Village", "Villa Maria Clara", "West Triangle", "White Plains"],
            "Makati": ["Bangkal", "Bel-Air", "Carmona", "Cembo", "Comembo", "Dasmariñas", "East Rembo", "Forbes Park", "Guadalupe Nuevo", "Guadalupe Viejo", "Kasilawan", "La Paz", "Magallanes", "Olympia", "Palanan", "Pembo", "Pinagkaisahan", "Pio del Pilar", "Pitogo", "Poblacion", "Post Proper Northside", "Post Proper Southside", "Rizal", "San Antonio", "San Isidro", "San Lorenzo", "Santa Cruz", "Singkamas", "South Cembo", "Tejeros", "Urdaneta", "Valenzuela", "West Rembo"],
            "Pasig City": ["Bagong Ilog", "Bagong Katipunan", "Bambang", "Buting", "Caniogan", "Dela Paz", "Kalawaan", "Kapasigan", "Kapitolyo", "Malinao", "Manggahan", "Maybunga", "Oranbo", "Palatiw", "Pinagbuhatan", "Pineda", "Rosario", "Sagad", "San Antonio", "San Joaquin", "San Jose", "San Miguel", "San Nicolas", "Santa Cruz", "Santa Lucia", "Santa Rosa", "Santolan", "Santo Tomas", "Sumilang", "Ugong"],
            "Taguig City": ["Bagumbayan", "Bambang", "Calzada", "Central Bicutan", "Central Signal Village", "Fort Bonifacio", "Hagonoy", "Ibayo-Tipas", "Katuparan", "Ligid-Tipas", "Lower Bicutan", "Maharlika Village", "Napindan", "New Lower Bicutan", "North Daang Hari", "North Signal Village", "Palingon", "Pinagsama", "San Miguel", "Santa Ana", "South Daang Hari", "South Signal Village", "Tanyag", "Tuktukan", "Upper Bicutan", "Ususan", "Wawa", "Western Bicutan"]
        }
    }
};

// Industry Types
const IndustryTypes = [
    "Agriculture, Forestry & Fishing",
    "Mining & Quarrying",
    "Manufacturing",
    "Construction",
    "Retail & Wholesale Trade",
    "Transportation & Storage",
    "Accommodation & Food Service",
    "Information & Communication",
    "Financial & Insurance",
    "Real Estate",
    "Professional Services",
    "Administrative & Support Services",
    "Education",
    "Healthcare & Social Work",
    "Arts, Entertainment & Recreation",
    "Business Process Outsourcing (BPO)",
    "Technology & Software",
    "Government & Public Administration",
    "Non-Profit & NGO",
    "Other Services"
];

// Subscription Plans
const SubscriptionPlans = [
    {
        level: 1,
        name: "Basic",
        description: "Essential time tracking for small teams",
        price: { monthly: 2999, yearly: 29990 },
        modules: ["time_attendance"],
        maxEmployees: 50,
        features: [
            "Time Tracking & Attendance",
            "Basic Reports",
            "Email Support",
            "Up to 50 employees"
        ]
    },
    {
        level: 2,
        name: "Standard",
        description: "Complete attendance with shift management",
        price: { monthly: 5999, yearly: 59990 },
        modules: ["time_attendance", "overtime_shift"],
        maxEmployees: 150,
        features: [
            "Time Tracking & Attendance",
            "Overtime & Shift Management",
            "Advanced Reports",
            "Priority Support",
            "Up to 150 employees"
        ]
    },
    {
        level: 3,
        name: "Professional",
        description: "Full payroll with government compliance",
        price: { monthly: 9999, yearly: 99990 },
        modules: ["time_attendance", "overtime_shift", "salary_compliance"],
        maxEmployees: 300,
        features: [
            "Time Tracking & Attendance",
            "Overtime & Shift Management",
            "Salary Computations",
            "SSS, PhilHealth, Pag-IBIG, Tax",
            "Compliance Reports",
            "Up to 300 employees"
        ]
    },
    {
        level: 4,
        name: "Enterprise",
        description: "Complete HR & Payroll solution",
        price: { monthly: 14999, yearly: 149990 },
        modules: ["time_attendance", "overtime_shift", "salary_compliance", "payroll_payslip", "employee_portal"],
        maxEmployees: 600,
        features: [
            "All Professional Features",
            "Payroll & Payslip Generation",
            "Employee Self-Service Portal",
            "Team Management",
            "Budget Allocation",
            "Incident Reports",
            "Dedicated Support",
            "Up to 600 employees"
        ]
    }
];

// Module Definitions
const Modules = {
    time_attendance: {
        id: "time_attendance",
        name: "Time Tracking & Attendance",
        icon: "bi-clock",
        description: "Track employee time-in/time-out, attendance records, and generate attendance reports.",
        color: "#10b981",
        default: true
    },
    overtime_shift: {
        id: "overtime_shift",
        name: "Overtime & Shift Management",
        icon: "bi-calendar-week",
        description: "Manage employee shifts, overtime requests, and scheduling with approval workflows.",
        color: "#f59e0b"
    },
    salary_compliance: {
        id: "salary_compliance",
        name: "Salary Computations with Government Compliance",
        icon: "bi-calculator",
        description: "Automated salary computation with SSS, PhilHealth, Pag-IBIG, and tax calculations.",
        color: "#6366f1"
    },
    payroll_payslip: {
        id: "payroll_payslip",
        name: "Payroll & Payslip with Benefits",
        icon: "bi-cash-stack",
        description: "Complete payroll processing, payslip generation, and benefits management.",
        color: "#ec4899"
    },
    employee_portal: {
        id: "employee_portal",
        name: "Employee Self-Service Portal",
        icon: "bi-person-workspace",
        description: "Employees can view payslips, request leaves, check attendance, and access HR services online.",
        color: "#8b5cf6"
    }
};

// Payment Methods
const PaymentMethods = [
    { id: "cash", name: "Cash Payment", icon: "bi-cash", online: false },
    { id: "gcash", name: "GCash (via PayMongo)", icon: "bi-phone", online: true },
    { id: "grab_pay", name: "GrabPay (via PayMongo)", icon: "bi-wallet2", online: true },
    { id: "card", name: "Credit/Debit Card (via PayMongo)", icon: "bi-credit-card", online: true },
    { id: "paymaya", name: "Maya (via PayMongo)", icon: "bi-credit-card-2-front", online: true },
    { id: "bank_transfer", name: "Bank Transfer (Manual)", icon: "bi-bank", online: false },
    { id: "check", name: "Check Payment", icon: "bi-file-earmark-text", online: false }
];

// Export for use
if (typeof module !== 'undefined' && module.exports) {
    module.exports = { PHLocations, IndustryTypes, SubscriptionPlans, Modules, PaymentMethods };
}
