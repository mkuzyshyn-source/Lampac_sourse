﻿using Lampac.Models.SISI;
using Lampac.Models.JAC;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.IO;
using Lampac.Models.LITE;
using System.Collections.Generic;
using Lampac.Models.DLNA;
using Lampac.Models.AppConf;
using System;
using System.Text.RegularExpressions;
using Lampac.Models.Merchant;
using Lampac.Models.Module;
using System.Reflection;

namespace Lampac
{
    public class AppInit
    {
        #region conf
        static (AppInit, DateTime) cacheconf = default;

        public static AppInit conf
        {
            get
            {
                if (!File.Exists("init.conf"))
                    return new AppInit();

                var lastWriteTime = File.GetLastWriteTime("init.conf");

                if (cacheconf.Item2 != lastWriteTime)
                {
                    string init = File.ReadAllText("init.conf");

                    // fix tracks.js
                    init = Regex.Replace(init, "\"ffprobe\": ?\"([^\"]+)?\"[^\n\r]+", "");

                    cacheconf.Item1 = JsonConvert.DeserializeObject<AppInit>(init);
                    cacheconf.Item2 = lastWriteTime;

                    if (File.Exists("merchant/users.txt"))
                    {
                        long utc = DateTime.UtcNow.ToFileTimeUtc();
                        foreach (string line in File.ReadAllLines("merchant/users.txt"))
                        {
                            if (string.IsNullOrWhiteSpace(line) && !line.Contains("@"))
                                continue;

                            var data = line.Split(',');
                            if (data.Length > 1)
                            {
                                if (long.TryParse(data[1], out long ex) && ex > utc)
                                    cacheconf.Item1.accsdb.accounts.Add(data[0].Trim().ToLower());
                            }
                        }
                    }
                }

                return cacheconf.Item1;
            }
        }

        public static string Host(HttpContext httpContext) => $"{httpContext.Request.Scheme}://{(string.IsNullOrWhiteSpace(conf.listenhost) ? httpContext.Request.Host.Value : conf.listenhost)}";
        #endregion

        #region AppInit
        public static List<RootModule> modules;

        static AppInit()
        {
            if (File.Exists("module/manifest.json"))
            {
                modules = new List<RootModule>();

                foreach (var mod in JsonConvert.DeserializeObject<List<RootModule>>(File.ReadAllText("module/manifest.json")))
                {
                    if (!mod.enable)
                        continue;

                    string path = File.Exists(mod.dll) ? mod.dll : $"{Environment.CurrentDirectory}/module/{mod.dll}";
                    if (File.Exists(path))
                    {
                        mod.assembly = Assembly.LoadFile(path);

                        try
                        {
                            if (mod.initspace != null && mod.assembly.GetType(mod.initspace) is Type t && t.GetMethod("loaded") is MethodInfo m)
                                m.Invoke(null, new object[] { });
                        }
                        catch { }

                        modules.Add(mod);
                    }
                }
            }
        }
        #endregion


        public string listenip = "any";

        public int listenport = 9118;

        public string listenhost = null;

        public FfprobeSettings ffprobe = new FfprobeSettings() { enable = true, os = "linux" };

        public ServerproxyConf serverproxy = new ServerproxyConf() { enable = true, encrypt = true, allow_tmdb = true };

        public bool multiaccess = false;

        public string anticaptchakey;


        public FileCacheConf fileCacheInactiveDay = new FileCacheConf() { html = 3, img = 1, torrent = 10 };

        public DLNASettings dlna = new DLNASettings() { enable = true };

        public WebConf LampaWeb = new WebConf() { autoupdate = true, index = "lampa-main/index.html" };

        public SisiConf sisi = new SisiConf() { heightPicture = 200 };

        public OnlineConf online = new OnlineConf() { findkp = "alloha", checkOnlineSearch = true };

        public AccsConf accsdb = new AccsConf() { cubMesage = "Войдите в аккаунт", denyMesage = "Добавьте {account_email} в init.conf", maxiptohour = 10 };

        public MerchantsModel Merchant = new MerchantsModel();

        public HashSet<Known> KnownProxies { get; set; }

        public JacConf jac = new JacConf();


        public TrackerSettings Rutor = new TrackerSettings("http://rutor.info", priority: "torrent");

        public TrackerSettings Megapeer = new TrackerSettings("http://megapeer.vip");

        public TrackerSettings TorrentBy = new TrackerSettings("http://torrent.by", priority: "torrent");

        public TrackerSettings Kinozal = new TrackerSettings("http://kinozal.tv");

        public TrackerSettings NNMClub = new TrackerSettings("https://nnmclub.to");

        public TrackerSettings Bitru = new TrackerSettings("https://bitru.org");

        public TrackerSettings Toloka = new TrackerSettings("https://toloka.to", enable: false);

        public TrackerSettings Rutracker = new TrackerSettings("https://rutracker.net", enable: false, priority: "torrent");

        public TrackerSettings Underverse = new TrackerSettings("https://underver.se", enable: false);

        public TrackerSettings Selezen = new TrackerSettings("https://selezen.org", enable: false, priority: "torrent");

        public TrackerSettings Lostfilm = new TrackerSettings("https://www.lostfilm.tv", enable: false);

        public TrackerSettings Anilibria = new TrackerSettings("https://www.anilibria.tv");

        public TrackerSettings Animelayer = new TrackerSettings("http://animelayer.ru", enable: false);

        public TrackerSettings Anifilm = new TrackerSettings("https://anifilm.tv");


        public SisiSettings BongaCams = new SisiSettings("https://rt.bongacams.com");

        public SisiSettings Chaturbate = new SisiSettings("https://chaturbate.com");

        public SisiSettings Ebalovo = new SisiSettings("https://www.ebalovo.pro");

        public SisiSettings Eporner = new SisiSettings("https://www.eporner.com");

        public SisiSettings HQporner = new SisiSettings("https://hqporner.com");

        public SisiSettings Porntrex = new SisiSettings("https://www.porntrex.com");

        public SisiSettings Spankbang = new SisiSettings("https://ru.spankbang.com");

        public SisiSettings Xhamster = new SisiSettings("https://ru.xhamster.com") { streamproxy = true };

        public SisiSettings Xnxx = new SisiSettings("https://www.xnxx.com");

        public SisiSettings Xvideos = new SisiSettings("https://www.xvideos.com");

        public SisiSettings PornHub = new SisiSettings("https://rt.pornhub.com");



        public OnlinesSettings Kinobase = new OnlinesSettings("https://kinobase.org");

        public OnlinesSettings Rezka = new OnlinesSettings("https://rezka.ag");

        public OnlinesSettings Voidboost = new OnlinesSettings("https://voidboost.net");

        public OnlinesSettings Collaps = new OnlinesSettings("https://api.delivembd.ws");

        public OnlinesSettings Ashdi = new OnlinesSettings("https://base.ashdi.vip");

        public OnlinesSettings Eneyida = new OnlinesSettings("https://eneyida.tv");

        public OnlinesSettings Kinokrad = new OnlinesSettings("https://kinokrad.cc");

        public OnlinesSettings Kinotochka = new OnlinesSettings("https://kinotochka.co");

        public OnlinesSettings Redheadsound = new OnlinesSettings("https://redheadsound.studio");

        public OnlinesSettings Kinoprofi = new OnlinesSettings("https://kinoprofi.vip", apihost: "https://api.kinoprofi.vip");

        public OnlinesSettings Lostfilmhd = new OnlinesSettings("http://www.disneylove.ru");

        public FilmixSettings Filmix = new FilmixSettings("http://filmixapp.cyou");

        public FilmixSettings FilmixPartner = new FilmixSettings("http://5.61.56.18/partner_api", enable: false);

        public OnlinesSettings Zetflix = new OnlinesSettings("https://zetfix.online");

        public OnlinesSettings VideoDB = new OnlinesSettings(null);

        public OnlinesSettings CDNmovies = new OnlinesSettings("https://cdnmovies.nl");


        public OnlinesSettings VCDN = new OnlinesSettings(null, apihost: "https://89442664434375553.svetacdn.in/0HlZgU1l1mw5");

        public OnlinesSettings VoKino = new OnlinesSettings("http://api.vokino.tv") { streamproxy = true };

        public OnlinesSettings VideoAPI = new OnlinesSettings("https://videoapi.tv", token: "qR0taraBKvEZULgjoIRj69AJ7O6Pgl9O");

        public IframeVideoSettings IframeVideo = new IframeVideoSettings("https://iframe.video", "https://videoframe.space");

        public HDVBSettings HDVB = new HDVBSettings("https://apivb.info", "5e2fe4c70bafd9a7414c4f170ee1b192");

        public OnlinesSettings Seasonvar = new OnlinesSettings(null, apihost: "http://api.seasonvar.ru");

        public KinoPubSettings KinoPub = new KinoPubSettings("https://api.service-kp.com") { uhd = true };

        public BazonSettings Bazon = new BazonSettings("https://bazon.cc", "", true);

        public AllohaSettings Alloha = new AllohaSettings("https://api.alloha.tv", "https://torso.as.alloeclub.com", "", "", true);

        public KodikSettings Kodik = new KodikSettings("https://kodikapi.com", "http://kodik.biz", "b7cc4293ed475c4ad1fd599d114f4435", "", true);


        public OnlinesSettings AnilibriaOnline = new OnlinesSettings("https://www.anilibria.tv", apihost: "https://api.anilibria.tv");

        public OnlinesSettings AniMedia = new OnlinesSettings("https://online.animedia.tv");

        public OnlinesSettings AnimeGo = new OnlinesSettings("https://animego.org");

        public OnlinesSettings Animevost = new OnlinesSettings("https://animevost.org");

        public OnlinesSettings Animebesst = new OnlinesSettings("https://anime1.animebesst.org");


        public ProxySettings proxy = new ProxySettings();

        public List<ProxySettings> globalproxy = new List<ProxySettings>() 
        {
            new ProxySettings() 
            {
                pattern = "\\.onion",
                list = new List<string>() { "socks5://127.0.0.1:9050" }
            }
        };


        public List<OverrideResponse> overrideResponse = new List<OverrideResponse>()
        {
            new OverrideResponse()
            {
                pattern = "/over/text",
                action = "html",
                type = "text/plain; charset=utf-8",
                val = "text"
            },
            new OverrideResponse()
            {
                pattern = "/over/online.js",
                action = "file",
                type = "application/javascript; charset=utf-8",
                val = "plugins/online.js"
            },
            new OverrideResponse()
            {
                pattern = "/over/gogoole",
                action = "redirect",
                val = "https://www.google.com/"
            }
        };
    }
}
