using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Zadatak3
{
    internal class Program
    {
        class Let
        {
            public string id;
            public string polaziste;
            public string odrediste;
            public int planiranoPoletanje;
            public int trajanje;
            public int gate;
            public int konacnoPoletanje;
            public int kasnjenje;
            public List<Putnik> poleteli = new List<Putnik>();
            public List<Putnik> propustili = new List<Putnik>();
        }

        class Putnik
        {
            public string id;
            public string imePrezime;
            public string idLeta;
            public int vremeDolaska;
            public bool poletio;
        }

        class Parametri
        {
            public int trajanjeBoardinga;
            public int kasnjenjePriPunomGateu;
            public int maxPutnikaPoGateu;
        }

        static void Main()
        {
            var letovi = UcitajLetove("letovi.txt");
            var putnici = UcitajPutnike("putnici.txt");
            var parametri = UcitajParametre("parametri.txt");

            int zatvorenGate = -1;
            int zatvaranjeOd = -1;
            int zatvaranjeDo = -1;
            int zamenskiGate = -1;


            Simuliraj(letovi, putnici, parametri, zatvorenGate, zatvaranjeOd, zatvaranjeDo, zamenskiGate);
            IspisiRezultate(letovi, parametri);
            while (true)
            {
                Console.WriteLine("Opcije: 1) ID leta 2) Destinacija 3) Kraj");
                Console.Write("Unos: ");
                var unos = Console.ReadLine();
                if (unos == "1")
                {
                    Console.Write("Unesite ID leta: ");
                    var id = Console.ReadLine();
                    IspisZaLet(letovi, id);
                }
                else if (unos == "2")
                {
                    Console.Write("Unesite destinaciju (npr. FRA): ");
                    var dest = Console.ReadLine();
                    IspisZaDestinaciju(letovi, dest);
                }
                else if (unos == "3")
                {
                    break;
                }
            }

            NapraviIzvestaj(letovi, parametri, "izvestaj_aerodrom.txt");


        }

        static List<Let> UcitajLetove(string putanja)
        {
            var lista = new List<Let>();
            foreach (var linija in File.ReadAllLines(putanja))
            {
                var t = linija.Split(';');
                if (t.Length < 6) continue;
                lista.Add(new Let
                {

                    id = t[0].Trim(),
                    polaziste = t[1].Trim(),
                    odrediste = t[2].Trim(),
                    planiranoPoletanje = int.Parse(t[3].Trim()),
                    trajanje = int.Parse(t[4].Trim()),
                    gate = int.Parse(t[5].Trim()),
                    konacnoPoletanje = int.Parse(t[3].Trim()),
                    kasnjenje = 0

                });
            }

            return lista;

        }

        static List<Putnik> UcitajPutnike(string putanja)
        {
            var lista = new List<Putnik>();
            foreach (var linija in File.ReadAllLines(putanja))
            {
                var t = linija.Split(';');
                if (t.Length < 4) continue;
                lista.Add(new Putnik
                {

                    id = t[0].Trim(),
                    imePrezime = t[1].Trim(),
                    idLeta = t[2].Trim(),
                    vremeDolaska = int.Parse(t[3].Trim()),
                    poletio = false
                });

            }

            return lista;

        }

        static Parametri UcitajParametre(string putanja)
        {

            var linija = File.ReadAllLines(putanja).FirstOrDefault();
            var t = linija.Split(';');
            return new Parametri
            {

                trajanjeBoardinga = int.Parse(t[0].Trim()),
                kasnjenjePriPunomGateu = int.Parse(t[1].Trim()),
                maxPutnikaPoGateu = int.Parse(t[2].Trim())

            };

        }

        static void Simuliraj(List<Let> letovi, List<Putnik> putnici, Parametri parametri, int zatvorenGate, int zatvaranjeOd, int zatvaranjeDo, int zamenskiGate)
        {

            int maxMin = Math.Max(
                letovi.Any() ? letovi.Max(l => l.planiranoPoletanje + l.trajanje) : 0,
                putnici.Any() ? putnici.Max(p => p.vremeDolaska) : 0
            ) + 300;


            var putniciNaAerodromu = new HashSet<Putnik>();
            var mapLet = letovi.ToDictionary(l => l.id, l => l);
            for (int m = 0; m <= maxMin; m++)
            {

                if (zatvorenGate != -1 && m == zatvaranjeOd)
                {

                    foreach (var l in letovi.Where(x => x.gate == zatvorenGate && x.planiranoPoletanje >= zatvaranjeOd && x.planiranoPoletanje <= zatvaranjeDo))
                    {

                        l.gate = zamenskiGate != -1 ? zamenskiGate : NadjiSlobodanGate(letovi, l);

                    }

                }

                foreach (var p in putnici.Where(x => x.vremeDolaska == m))
                {
                    putniciNaAerodromu.Add(p);
                }

                foreach (var let in letovi)
                {

                    int boardingStart = let.planiranoPoletanje - parametri.trajanjeBoardinga;
                    if (boardingStart < 0) boardingStart = 0;


                    if (m == boardingStart && let.kasnjenje == 0)
                    {

                        int prisutnoKodGatea = putniciNaAerodromu.Count(pp => mapLet.ContainsKey(pp.idLeta) && mapLet[pp.idLeta].gate == let.gate);
                        if (prisutnoKodGatea > parametri.maxPutnikaPoGateu)
                        {

                            let.kasnjenje += parametri.kasnjenjePriPunomGateu;
                            let.konacnoPoletanje = let.planiranoPoletanje + let.kasnjenje;

                        }

                    }

                    int boardingEnd = let.konacnoPoletanje - 1;
                    foreach (var p in putniciNaAerodromu.Where(x => x.idLeta == let.id))
                    {

                        if (!p.poletio)
                        {

                            if (p.vremeDolaska <= boardingEnd)
                            {

                                p.poletio = true;
                                let.poleteli.Add(p);
                            }

                        }

                    }


                    if (m == let.konacnoPoletanje)
                    {

                        var ostali = putniciNaAerodromu.Where(x => x.idLeta == let.id && !x.poletio).ToList();
                        foreach (var p in ostali)
                        {
                            let.propustili.Add(p);
                        }
                        foreach (var p in putniciNaAerodromu.Where(x => x.idLeta == let.id).ToList())
                        {
                            putniciNaAerodromu.Remove(p);
                        }

                    }

                }
            }
        }

        static int NadjiSlobodanGate(List<Let> letovi, Let let)
        {
            var sviGateovi = letovi.Select(l => l.gate).Distinct().OrderBy(x => x).ToList();
            for (int g = 1; g <= 99; g++)
            {

                if (!sviGateovi.Contains(g)) return g;

            }

            return sviGateovi.Max() + 1;

        }

        static void IspisiRezultate(List<Let> letovi, Parametri parametri)
        {
            foreach (var l in letovi.OrderBy(x => x.planiranoPoletanje))
            {
                Console.WriteLine($"{l.id};poleteli:{l.poleteli.Count};propustili:{l.propustili.Count};konacno_poletanje:{l.konacnoPoletanje}");
            }

            double prosecnoKasnjenje = letovi.Any() ? letovi.Average(x => x.kasnjenje) : 0;
            Console.WriteLine($"prosecno_kasnjenje:{prosecnoKasnjenje:F2}");
            var maxKasnjenje = letovi.OrderByDescending(x => x.kasnjenje).FirstOrDefault();
            if (maxKasnjenje != null)
            {
                Console.WriteLine($"najvece_kasnjenje_let:{maxKasnjenje.id};kasnjenje:{maxKasnjenje.kasnjenje}");
            }

        }

        static void IspisZaLet(List<Let> letovi, string id)
        {
            var l = letovi.FirstOrDefault(x => x.id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (l == null)
            {
                Console.WriteLine("Nepostojeci let");
                return;
            }
            Console.WriteLine("Poleteli:");
            foreach (var p in l.poleteli.OrderBy(x => x.id))
            {
                Console.WriteLine($"{p.id};{p.imePrezime}");
            }
            Console.WriteLine("Propustili:");
            foreach (var p in l.propustili.OrderBy(x => x.id))
            {
                Console.WriteLine($"{p.id};{p.imePrezime}");
            }

        }

        static void IspisZaDestinaciju(List<Let> letovi, string dest)
        {
            int ukupno = letovi.Where(x => x.odrediste.Equals(dest, StringComparison.OrdinalIgnoreCase)).Sum(x => x.poleteli.Count);
            Console.WriteLine($"ukupno_putnika_za_{dest}:{ukupno}");
        }

        static void NapraviIzvestaj(List<Let> letovi, Parametri parametri, string putanja)
        {

            int ukupnoPutnika = letovi.Sum(x => x.poleteli.Count) + letovi.Sum(x => x.propustili.Count);
            double prosecnoKasnjenje = letovi.Any() ? letovi.Average(x => x.kasnjenje) : 0;
            var grupisanoPoGateu = letovi.GroupBy(x => x.gate).Select(g => new { gate = g.Key, brojLetova = g.Count(), putnika = g.Sum(l => l.poleteli.Count) }).OrderByDescending(x => x.putnika).ToList();
            var najzaguseniji = grupisanoPoGateu.FirstOrDefault();
            using (var sw = new StreamWriter(putanja))
            {

                sw.WriteLine($"ukupno_putnika:{ukupnoPutnika}");
                sw.WriteLine($"prosecno_kasnjenje:{prosecnoKasnjenje:F2}");
                if (najzaguseniji != null)
                {
                    sw.WriteLine($"najzaguseniji_gate:{najzaguseniji.gate};putnika:{najzaguseniji.putnika};letova:{najzaguseniji.brojLetova}");
                }
                var maxKasnjenje = letovi.OrderByDescending(x => x.kasnjenje).FirstOrDefault();
                if (maxKasnjenje != null)
                {
                    sw.WriteLine($"najvece_kasnjenje_let:{maxKasnjenje.id};kasnjenje:{maxKasnjenje.kasnjenje}");
                }


            }


        }
    }
}
