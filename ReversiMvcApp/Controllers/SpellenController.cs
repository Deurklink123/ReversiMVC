using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReversiMvcApp.Data;
using ReversiMvcApp.Enums;
using ReversiMvcApp.Models;


namespace ReversiMvcApp.Controllers
{
    public class SpellenController : Controller
    {
        private readonly SpellenDbContext _context;
        private readonly ReversiDbContext _SpelerContext;
        static readonly HttpClient client = new HttpClient();
        public SpellenController(SpellenDbContext context, ReversiDbContext SpelerContext)
        {
            _context = context;
            _SpelerContext = SpelerContext;
        }

        
        // GET: Spellen
        public async Task<IActionResult> Index()
        {
            var redirect = await OnLoad();
            if (redirect != null)
            {
                return redirect;
            }

            List<Spel> spellen = new List<Spel>();
            try
            {
                //using HttpResponseMessage response = await client.GetAsync("https://localhost:5001/api/Spel");
                //response.EnsureSuccessStatusCode();
                //var responseBody = await response.Content.ReadAsStringAsync();

                string responseBody = await client.GetStringAsync("https://localhost:5001/api/Spel");

                Dictionary<string, string> spellenWachtend = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseBody);

                foreach (var spel in spellenWachtend)
                {
                    var x = new Spel();
                    x.token = spel.Key;
                    x.omschrijving = spel.Value;
                    x.bord = null;
                    x.aanDeBeurt = "0";
                    spellen.Add(x);
                }

            }
            catch (HttpRequestException e)
            {
                throw new Exception("Message :{0} " + e.Message);
            }

            return View(spellen);
        }

        // GET: Spellen/SpelPagina/5
        public async Task<IActionResult> SpelPagina(string id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var query = new Dictionary<string, string>
            {
                ["SpelToken"] = id,
            };

            string responseBody = await client.GetStringAsync(QueryHelpers.AddQueryString("https://localhost:5001/api/Spel/Spel", query));
            Spel spel = JsonConvert.DeserializeObject<Spel>(responseBody);
            dynamic data = JObject.Parse(responseBody);


            /*            string[] split1 = responseBody.Split("\"Bord\"");
                        string[] split2 = split1[1].Split("]],");
                        string noBord = split1[0] + split2[1];
                        Dictionary<string, string> spelDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(noBord);

                        string bord = "{\"Bord\"" + split2[0] + "]]}";
                        Dictionary<string, Kleur[,]> bordDict = JsonConvert.DeserializeObject<Dictionary<string, Kleur[,]>>(bord);

                        var spel = new Spel();
                        spel.token = spelDict["Token"];
                        spel.omschrijving = spelDict["Omschrijving"];
                        spel.bord = bordDict["Bord"];
                        spel.bordString = spelDict["BordString"];
                        spel.aanDeBeurt = spelDict["AandeBeurt"];*/


            if (spel == null)
            {
                return NotFound();
            }

            ClaimsPrincipal currentUser = this.User;
            var currentUserID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (currentUserID != data.Speler1Token.ToString())
            {
                //throw new Exception("idk beter doet dit iets");

                var SpelPath = "https://localhost:5001/api/Spel/JoinSpel";

                var load = new Dictionary<string, string>();
                load.Add("SpelToken", spel.token);
                load.Add("SpelerToken", currentUserID);

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, SpelPath) { Content = new FormUrlEncodedContent(load) };
                var res = await client.SendAsync(requestMessage);

                ViewData["Speler2"] = currentUserID;
            }
            else
            {
                ViewData["Speler1"] = currentUserID;
            }


            //then iets doen op popup te tonen bij speler 1 en speler 2 naar wacht pagina sturen met draaiend ding ofz
            //of javascript alarm melding geven dat hij moet wachten ofzo idk

            return View(spel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SpelPagina(string token, string eindstand)
        {
            var query = new Dictionary<string, string>
            {
                ["SpelToken"] = token
            };
            string response = await client.GetStringAsync(QueryHelpers.AddQueryString("https://localhost:5001/api/Spel/Spel", query));


            if (response != "null")
            {
                string[] split1 = response.Split("\"Bord\"");
                string[] split2 = split1[1].Split("]],");
                string noBord = split1[0] + split2[1];
                Dictionary<string, string> spelDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(noBord);

                var speler1 = await _SpelerContext.Spelers.FindAsync(spelDict["Speler1Token"]);
                var speler2 = await _SpelerContext.Spelers.FindAsync(spelDict["Speler2Token"]);
                if (eindstand == "wit")
                {
                    //speler1 winst +1
                    speler1.AantalGewonnen++;

                    //speler2 verloren +1
                    speler2.AantalVerloren++;
                }
                else if (eindstand == "zwart")
                {
                    //speler2 winst +1
                    speler2.AantalGewonnen++;

                    //speler1 verloren +1
                    speler1.AantalVerloren++;
                }
                else
                {
                    //speler1 gelijk +1
                    speler1.AantalGelijk++;

                    //speler2 gelijk +1
                    speler2.AantalGelijk++;
                }

                await _SpelerContext.SaveChangesAsync();

                //spel deleten
                var load = new Dictionary<string, string>();
                load.Add("SpelToken", token);

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://localhost:5001/api/Spel/DeleteSpel") { Content = new FormUrlEncodedContent(load) };
                var res = await client.SendAsync(requestMessage);

            }

            return RedirectToAction(nameof(Index));
        }



            // GET: Spellen/Create
            public async Task<IActionResult> CreateAsync()
        {
            var redirect = await OnLoad();
            if (redirect != null)
            {
                return redirect;
            }
            return View();
        }

        // POST: Spellen/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("omschrijving")] string omschrijving)
        {

            if (ModelState.IsValid)
            {
                var SpelPath = "https://localhost:5001/api/Spel/MakeSpel";

                ClaimsPrincipal currentUser = this.User; 
                var currentUserID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;

                var load = new Dictionary<string, string>();
                load.Add("Speler1Token", currentUserID);
                load.Add("Omschrijving", omschrijving);

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, SpelPath) { Content = new FormUrlEncodedContent(load) };
                var res = await client.SendAsync(requestMessage);

                return RedirectToAction(nameof(Index));
            }
            return View();
        }


        public async Task<IActionResult> Score()
        {
            ClaimsPrincipal currentUser = this.User;
            if (currentUser.Identity.IsAuthenticated == false)
            {
                return Redirect($"/Identity/Account/Login");
            }
            var currentUserID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            //throw new Exception(currentUserID);
            var speler = await _SpelerContext.Spelers
                .FirstOrDefaultAsync(m => m.Guid == currentUserID);
            if (speler == null)
            {
                return NotFound();
            }

            return View(speler);
        }


        public async Task<RedirectResult> OnLoad()
        {
            ClaimsPrincipal currentUser = this.User;
            if (currentUser.Identity.IsAuthenticated == true)
            {
                var currentUserID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;

                var query = new Dictionary<string, string>
                {
                    ["SpelerToken"] = currentUserID
                };
                string response = await client.GetStringAsync(QueryHelpers.AddQueryString("https://localhost:5001/api/Spel/SpelSpeler", query));

                if (response != "null")
                {
                    Spel spel = JsonConvert.DeserializeObject<Spel>(response);
                    /*string[] split1 = response.Split("\"Bord\"");
                    string[] split2 = split1[1].Split("]],");
                    string noBord = split1[0] + split2[1];
                    Dictionary<string, string> spelDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(noBord);*/

                return Redirect($"/Spellen/SpelPagina/{spel.token}");
                }
            }
            return null;
        }










        // GET: Spellen/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var spel = await _context.Spellen.FindAsync(id);
            if (spel == null)
            {
                return NotFound();
            }
            return View(spel);
        }

        // POST: Spellen/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("omschrijving,bord,aanDeBeurt")] Spel spel)
        {
            if (id != spel.omschrijving)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(spel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SpelExists(spel.omschrijving))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(spel);
        }

        // GET: Spellen/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var spel = await _context.Spellen
                .FirstOrDefaultAsync(m => m.omschrijving == id);
            if (spel == null)
            {
                return NotFound();
            }

            return View(spel);
        }

        // POST: Spellen/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var spel = await _context.Spellen.FindAsync(id);
            _context.Spellen.Remove(spel);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SpelExists(string id)
        {
            return _context.Spellen.Any(e => e.omschrijving == id);
        }
    }
}
