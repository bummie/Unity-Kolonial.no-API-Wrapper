using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace KolonialAPI
{
    public class APIHandler
    {
        public JSONObject UserData { get; set; }
        private static APIHandler _api;
        private const string BASE_URL_VERSION = "v1";
        private const string BASE_URL_DOMAIN = "https://kolonial.no/api/" + BASE_URL_VERSION + "/";

        private string _sessionId = null;
        private APIHandler(){}

        public static APIHandler GetInstance()
        {
            if(APIHandler._api == null)
            {
                _api = new APIHandler();
            }

            return _api;
        }
        
        /// <summary>
        /// Returns sessionid and information about user
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public IEnumerator AuthenticateUser(string username, string password, Action<JSONObject> callbackFunction)
        {
            if (UserLoggedIn()) { yield return null; }
            WWWForm credentialForm = new WWWForm();
            credentialForm.AddField("username", username);
            credentialForm.AddField("password", password);

            string url = BASE_URL_DOMAIN + "user/login/";
            using (UnityWebRequest httpHandler = UnityWebRequest.Post(url, credentialForm))
            {
                AddHeader(httpHandler);

                yield return httpHandler.SendWebRequest();

                if (httpHandler.isNetworkError || httpHandler.isHttpError)
                {
                    Debug.Log(httpHandler.error);
                    callbackFunction(null);
                }
                else
                {
                    JSONObject responseJSONObject = JSON.Parse(httpHandler.downloadHandler.text).AsObject;
                    UserData = responseJSONObject;
                    _sessionId = responseJSONObject["sessionid"];
                    callbackFunction(responseJSONObject);
                }
            }
        }

        /// <summary>
        /// Logs the user out
        /// </summary>
        public void LogoutUser()
        {
            _sessionId = null;
        }

        /// <summary>
        /// Returns true if user is logged in and false if not
        /// </summary>
        /// <returns></returns>
        public bool UserLoggedIn()
        {
            return (_sessionId != null) ? true : false;
        }

        /// <summary>
        /// Returns items in cart and total price
        /// </summary>
        /// <returns></returns>
        public IEnumerator FetchCart(Action<JSONObject> callbackFunction)
        {
            string url = BASE_URL_DOMAIN + "cart/";
            using (UnityWebRequest httpHandler = UnityWebRequest.Get(url))
            {
                SetSessionId(httpHandler, _sessionId);
                AddHeader(httpHandler);

                yield return httpHandler.SendWebRequest();

                if (httpHandler.isNetworkError || httpHandler.isHttpError)
                {
                    Debug.Log(httpHandler.error);
                    callbackFunction(null);
                }
                else
                {
                    JSONObject responseJSONObject = JSON.Parse(httpHandler.downloadHandler.text).AsObject;
                    callbackFunction(responseJSONObject);
                }
            }
        }

        /// <summary>
        /// Updates the cart of the user based on session id, input is jsonobject format -> {"items": [{"product_id": 9329, "quantity": 2}]}
        /// </summary>
        /// <param name="cartItems"></param>
        /// <param name="callbackFunction"></param>
        /// <returns></returns>
        public IEnumerator UpdateCart(JSONObject cartItems, Action<JSONObject> callbackFunction)
        {
            byte[] cartItemsData = System.Text.Encoding.UTF8.GetBytes(cartItems.ToString());

            string url = BASE_URL_DOMAIN + "cart/items/";
            using (UnityWebRequest httpHandler = UnityWebRequest.Put(url, cartItemsData))
            {
                httpHandler.method = "POST";
                SetSessionId(httpHandler, _sessionId);
                AddJsonContentHeader(httpHandler);
                AddHeader(httpHandler);

                yield return httpHandler.SendWebRequest();

                if (httpHandler.isNetworkError || httpHandler.isHttpError)
                {
                    Debug.Log(httpHandler.responseCode + ": " + httpHandler.downloadHandler.text);
                    callbackFunction(null);
                }
                else
                {
                    JSONObject responseJSONObject = JSON.Parse(httpHandler.downloadHandler.text).AsObject;
                    callbackFunction(responseJSONObject);
                }
            }
        }

        /// <summary>
        /// Turns productid and quanity into jsonformat for API
        /// </summary>
        /// <param name="id"></param>
        /// <param name="quanity"></param>
        /// <returns></returns>
        public JSONObject ProductIdToJSON(int id, int quantity)
        {
            JSONObject baseObj = new JSONObject();
            baseObj["items"] = new JSONArray();

            JSONObject item = new JSONObject();
            item["product_id"] = id;
            item["quantity"] = quantity;
            baseObj["items"].Add(item);

            Debug.Log(baseObj.ToString());
            return baseObj;
        }

        /// <summary>
        /// Returns information about product from given productid
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public IEnumerator FetchProductFromId(int productId, Action<JSONObject> callbackFunction)
        {
            string url = BASE_URL_DOMAIN + "products/" + productId + "/";
            using (UnityWebRequest httpHandler = UnityWebRequest.Get(url))
            {
                AddHeader(httpHandler);

                yield return httpHandler.SendWebRequest();

                if (httpHandler.isNetworkError || httpHandler.isHttpError)
                {
                    Debug.Log(httpHandler.error);
                    callbackFunction(null);
                }
                else
                {
                    JSONObject responseJSONObject = JSON.Parse(httpHandler.downloadHandler.text).AsObject;
                    callbackFunction(responseJSONObject);
                }
            }
        }

        /// <summary>
        /// Returns all categories with child categories
        /// </summary>
        /// <returns></returns>
        public IEnumerator FetchCategories(Action<JSONObject> callbackFunction)
        {
            string url = BASE_URL_DOMAIN + "productcategories/";
            using (UnityWebRequest httpHandler = UnityWebRequest.Get(url))
            {
                AddHeader(httpHandler);

                yield return httpHandler.SendWebRequest();

                if (httpHandler.isNetworkError || httpHandler.isHttpError)
                {
                    Debug.Log(httpHandler.error);
                    callbackFunction(null);
                }
                else
                {
                    JSONObject responseJSONObject = JSON.Parse(httpHandler.downloadHandler.text).AsObject;
                    callbackFunction(responseJSONObject);
                }
            }
        }

        /// <summary>
        /// Returns products for a given category
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public IEnumerator FetchProductsFromCategoryId(int categoryId, Action<JSONObject> callbackFunction)
        {
            string url = BASE_URL_DOMAIN + "productcategories/" + categoryId + "/";
            using (UnityWebRequest httpHandler = UnityWebRequest.Get(url))
            {
                AddHeader(httpHandler);

                yield return httpHandler.SendWebRequest();

                if (httpHandler.isNetworkError || httpHandler.isHttpError)
                {
//                    Debug.Log(httpHandler.error);
                    callbackFunction(null);
                }
                else
                {
                    JSONObject responseJSONObject = JSON.Parse(httpHandler.downloadHandler.text).AsObject;
                    callbackFunction(responseJSONObject);
                }
            }
        }

        /// <summary>
        /// Returns products based on search
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        public IEnumerator SearchProducts(string search, Action<JSONObject> callbackFunction)
        {
            string url = BASE_URL_DOMAIN + "search/?q=" + search;

            using (UnityWebRequest httpHandler = UnityWebRequest.Get(url))
            {
                AddHeader(httpHandler);

                yield return httpHandler.SendWebRequest();

                if (httpHandler.isNetworkError || httpHandler.isHttpError)
                {
                    Debug.Log(httpHandler.error);
                    callbackFunction(null);
                }
                else
                {
                    JSONObject responseJSONObject = JSON.Parse(httpHandler.downloadHandler.text).AsObject;
                    callbackFunction(responseJSONObject);
                }
            }
        }

        /// <summary>
        /// Adds user agent and token

        /// </summary>
        /// <param name="httpHandler"></param>
        private void AddHeader(UnityWebRequest httpHandler)
        {
            // Header
            httpHandler.SetRequestHeader("User-Agent", ApiCred.USER_AGENT);
            httpHandler.SetRequestHeader("X-Client-Token", ApiCred.TOKEN);
        }

        /// <summary>
        /// Adds content types and accept json
        /// </summary>
        /// <param name="httpHandler"></param>
        private void AddJsonContentHeader(UnityWebRequest httpHandler)
        {
            // Header
            httpHandler.SetRequestHeader("Accept", "application/json");
            httpHandler.SetRequestHeader("Content-Type", "application/json");
        }

        /// <summary>
        /// Adds a cookie with given sessionId by header
        /// </summary>
        /// <param name="httpHandler"></param>
        /// <param name="sessionId"></param>
        private void SetSessionId(UnityWebRequest httpHandler, string sessionId)
        {
            if(_sessionId != null)
                httpHandler.SetRequestHeader("Cookie", "sessionid=" + sessionId);
        }

    }
}