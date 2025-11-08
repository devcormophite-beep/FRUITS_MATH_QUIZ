using UnityEngine;
using GoogleMobileAds.Api;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;
using Unity.VisualScripting;

public class AdmobAdsScript : MonoBehaviour
{
    public UnityEvent triger_On_show_banner;
    public UnityEvent triger_On_destroy_banner;
    public TextMeshProUGUI totalCoinsTxt;
    public UnityEvent triger_On_initial;
    public UnityEvent triger_On_load_rewards;
    public UnityEvent triger_On_load_intersticiel;
    public UnityEvent triger_On_load_BANNER;
    public UnityEvent triger_On_show_rewards;
    public UnityEvent triger_On_show_intersticiel;
    public UnityEvent triger_RELOAD_intersticiel;
    public UnityEvent triger_RELOAD_REWARDS;
    public UnityEvent triger_RELOAD_BANNER;

    public string appId = "ca-app-pub-4807504760191424~4158046519";// "ca-app-pub-3940256099942544~3347511713";


#if UNITY_ANDROID
    string bannerId = "ca-app-pub-4807504760191424/7794039197";
    string interId = "ca-app-pub-4807504760191424/9115408465";
    string rewardedId = "ca-app-pub-4807504760191424/6489245120";
    string nativeId = "ca-app-pub-3940256099942544/2247696110";

#elif UNITY_IPHONE
    string bannerId = "ca-app-pub-3940256099942544/2934735716";
    string interId = "ca-app-pub-3940256099942544/4411468910";
    string rewardedId = "ca-app-pub-3940256099942544/1712485313";
    string nativeId = "ca-app-pub-3940256099942544/3986624511";

#endif

    BannerView bannerView;
    InterstitialAd interstitialAd;
    RewardedAd rewardedAd;


    private void Start()
    {
        ShowCoins();
        MobileAds.RaiseAdEventsOnUnityMainThread = true;
        MobileAds.Initialize(initStatus => {

            print("initialisation reussi");
            CustomEvent.Trigger(gameObject, "triger_On_initial");

            


        });
    }

    #region Banner

    public void LoadBannerAd() {
        //create a banner
        CreateBannerView();

        //listen to banner events
        ListenToBannerEvents();

        //load the banner
        if (bannerView==null)
        {
            CreateBannerView();
        }

        var adRequest = new AdRequest();
        adRequest.Keywords.Add("unity-admob-sample");

        print("Loading banner Ad !!");
        bannerView.LoadAd(adRequest);//show the banner on the screen
        CustomEvent.Trigger(gameObject, "triger_On_load_BANNER");
    }
    void CreateBannerView() {

        if (bannerView!=null)
        {
            DestroyBannerAd();
        }
        bannerView = new BannerView(bannerId, AdSize.Banner, AdPosition.Top);
    }
    void ListenToBannerEvents() 
    {
        bannerView.OnBannerAdLoaded += () =>
        {
            Debug.Log("Banner view loaded an ad with response : "
                + bannerView.GetResponseInfo());
        };
        // Raised when an ad fails to load into the banner view.
        bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
            Debug.LogError("Banner view failed to load an ad with error : "
                + error);
            CustomEvent.Trigger(gameObject, "triger_RELOAD_BANNER");
        };
        // Raised when the ad is estimated to have earned money.
        bannerView.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log("Banner view paid {0} {1}."+
                adValue.Value+
                adValue.CurrencyCode);
        };
        // Raised when an impression is recorded for an ad.
        bannerView.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Banner view recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        bannerView.OnAdClicked += () =>
        {
            Debug.Log("Banner view was clicked.");
        };
        // Raised when an ad opened full screen content.
        bannerView.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Banner view full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        bannerView.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Banner view full screen content closed.");
        };
    }
    public void DestroyBannerAd() {

        if (bannerView!=null)
        {
            print("Destroying banner Ad");
            bannerView.Destroy();
            bannerView = null;
        }
    }
    #endregion

    #region Interstitial

    public void LoadInterstitialAd() {

        if (interstitialAd!=null)
        {
            interstitialAd.Destroy();
            interstitialAd = null;
        }
        var adRequest = new AdRequest();
        adRequest.Keywords.Add("unity-admob-sample");

        InterstitialAd.Load(interId, adRequest, (InterstitialAd ad, LoadAdError error) =>
        {
              if (error!=null||ad==null)
              {
                print("Interstitial ad failed to load"+error);
                CustomEvent.Trigger(gameObject, "triger_RELOAD_intersticiel");
                return;
              }

            print("Interstitial ad loaded !!"+ad.GetResponseInfo());
            CustomEvent.Trigger(gameObject, "triger_On_load_intersticiel");////////////////////////////////////INTERSTICIEL LOADES///////////////////

            interstitialAd = ad;
            InterstitialEvent(interstitialAd);
        });

    }
    public void ShowInterstitialAd() {

        if (interstitialAd != null && interstitialAd.CanShowAd())
        {
            interstitialAd.Show();
            CustomEvent.Trigger(gameObject, "triger_On_show_intersticiel");///////////////////////////////////////////intersticiel show//////////
        }
        else {
            print("Intersititial ad not ready!!");
        }
    }
    public void InterstitialEvent(InterstitialAd ad) {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) => 
        {
            Debug.Log("Interstitial ad paid {0} {1}."+
                adValue.Value+
                adValue.CurrencyCode);
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Interstitial ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            Debug.Log("Interstitial ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Interstitial ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Interstitial ad full screen content closed.");
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Interstitial ad failed to open full screen content " +
                           "with error : " + error);
        };
    }

    #endregion

    #region Rewarded

    public void LoadRewardedAd() {

        if (rewardedAd!=null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }
        var adRequest = new AdRequest();
        adRequest.Keywords.Add("unity-admob-sample");

        RewardedAd.Load(rewardedId, adRequest, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                print("Rewarded failed to load"+error);
                CustomEvent.Trigger(gameObject, "triger_RELOAD_REWARDS");
                return;
            }

            print("Rewarded ad loaded !!");
            CustomEvent.Trigger(gameObject, "triger_On_load_rewards");/////////////////////////////rewarded  load ///////////////////////
            rewardedAd = ad;
            RewardedAdEvents(rewardedAd);
           
        });
    }
    public void ShowRewardedAd() {

        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            rewardedAd.Show((Reward reward) =>
            {
                print("Give reward to player !!");
                CustomEvent.Trigger(gameObject, "triger_On_show_rewards"); ///////////////GIFT POUR LE JOUEUR /////////////////////////////////////
                GrantCoins(100);

            });
        }
        else {
            print("Rewarded ad not ready");
        }
    }
    public void RewardedAdEvents(RewardedAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log("Rewarded ad paid {0} {1}."+
                adValue.Value+
                adValue.CurrencyCode);
        };
        // Raised when an impression is recorded for an ad.
        ad.OnAdImpressionRecorded += () =>
        {
            Debug.Log("Rewarded ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        ad.OnAdClicked += () =>
        {
            Debug.Log("Rewarded ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("Rewarded ad full screen content opened.");
        };
        // Raised when the ad closed full screen content.
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Rewarded ad full screen content closed.");
        };
        // Raised when the ad failed to open full screen content.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded ad failed to open full screen content " +
                           "with error : " + error);
        };
    }

    #endregion


    
    #region extra 

    void GrantCoins(int coins) {
        int crrCoins = PlayerPrefs.GetInt("totalCoins");
        crrCoins += coins;
        PlayerPrefs.SetInt("totalCoins", crrCoins);

        ShowCoins();
    }
    void ShowCoins() {
        totalCoinsTxt.text = PlayerPrefs.GetInt("totalCoins").ToString();
    }

    #endregion

}
