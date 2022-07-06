using Gateway.Helper;
using Gateway.Models.Base;
using Newtonsoft.Json;

namespace Gateway.Services
{
    public class SettingService
    {
        private readonly string filename = "settings.json";
        private Settings settings;
        private RWLock locker = new RWLock();

        public SettingService()
        {
            Load();
        }

        public Settings GetSettings()
        {
            using (locker.ReadLock())
                return settings.Clone();
        }

        public bool SetSettings(Settings data)
        {
            using (locker.WriteLock())
            {
                settings.IsEnabled = data.IsEnabled;
                settings.IsHostedTransactionCheckerEnabled = data.IsHostedTransactionCheckerEnabled;
                settings.IsCreateTransactionsEnabled = data.IsCreateTransactionsEnabled;

                if (data.MerchantsProducts != null && data.MerchantsProducts.Any())
                    //if (settings.MerchantsProducts != null && settings.MerchantsProducts.Any())
                        settings.MerchantsProducts = data.MerchantsProducts.CloneList();
                //foreach (var product in data.MerchantsProducts)
                //{
                //    if (settings.MerchantsProducts.Any(x => x.Id == product.Id))
                //    {
                //        //change
                //        settings.MerchantsProducts[settings.MerchantsProducts.FindIndex(x => x.Id == product.Id)] = product;
                //    }
                //    else
                //    {
                //        //add new
                //        var last = settings.MerchantsProducts.Last();
                //        product.Id = last != null ? last.Id + 1 : 1;
                //        settings.MerchantsProducts.Add(product);
                //    }
                //}

                if (data.MerchantsExpenses != null && data.MerchantsExpenses.Any())
                    //if (settings.MerchantsExpenses != null && settings.MerchantsExpenses.Any())
                        settings.MerchantsExpenses = data.MerchantsExpenses.CloneList();
            }

            Save();

            return true;
        }

        public void Load()
        {
            using (locker.WriteLock())
            {
                if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), filename)))
                {
                    string text = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), filename));
                    if (!string.IsNullOrEmpty(text))
                        settings = JsonConvert.DeserializeObject<Settings>(text);
                }
                else
                {
                    settings = Settings.NewSettings();
                    string text = JsonConvert.SerializeObject(settings);
                    File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), filename), text);
                }
            }
        }

        public void Save()
        {
            using (locker.ReadLock())
            {
                string text = JsonConvert.SerializeObject(settings);
                File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), filename), text);
            }
        }
    }
}
