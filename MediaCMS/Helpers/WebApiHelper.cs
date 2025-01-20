using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows;
using System.Xml.Linq;
using WpfApp11.Helpers;
using System.Text.RegularExpressions;
using System.Diagnostics;

public class WebApiHelper
{
    private static WebApiHelper _instance;
    private static readonly object _lock = new object();
    private readonly HttpClient _httpClient;
    private string _baseUrl;
    private string _username= "administrator";
    private string _password= "password";

    private WebApiHelper()
    {
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        setData();
    }

    public static WebApiHelper Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new WebApiHelper();
                        
                    }
                }
            }
            return _instance;
        }
    }
    void setData()
    {
        var main = Application.Current.MainWindow as MediaCMS.MainWindow;
        _username = main.web_name;
        _password = main.web_pw;

    }

   

    public void Initialize(string baseUrl, string username, string password)
    {
        _baseUrl = baseUrl;
        _username = username;
        _password = password;





    }

    private async Task<string> PostAsync(string endpoint, Dictionary<string, string> additionalParams)
    {
        var parameters = new Dictionary<string, string>
        {
            {"usr", _username},
            {"pwd", _password}
        };

        foreach (var param in additionalParams)
        {
            parameters.Add(param.Key, param.Value);
        }

        var content = new FormUrlEncodedContent(parameters);

        HttpResponseMessage response = await _httpClient.PostAsync($"{_baseUrl}{endpoint}", content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    private async Task<string> GetAsync(string endpoint, Dictionary<string, string> additionalParams)
    {
        var parameters = new Dictionary<string, string>
        {
            {"usr", _username},
            {"pwd", _password}
        };

        foreach (var param in additionalParams)
        {
            parameters.Add(param.Key, param.Value);
        }

        var queryString = await new FormUrlEncodedContent(parameters).ReadAsStringAsync();
        HttpResponseMessage response = await _httpClient.GetAsync($"{_baseUrl}{endpoint}?{queryString}");
        Logger.Log2($"{_baseUrl}{endpoint}?{queryString}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    // PDU 상태체크
    public async Task<Dictionary<string, string>> StatusPDU(string url, string index)
    {
        try
        {
            _baseUrl = $"http://{url}";
            string response = await GetAsync("/api/outlet/relay", new Dictionary<string, string>
            {
                {"index", index}
            });
            Logger.Log2($"response : {response}");

            //string response = "<OutletRelay>< 02 > ON < 02 ></ OutletRelay >".Trim();
            response = Regex.Replace(response, @"\s+", "");

            // 값 추출
            Match match = Regex.Match(response, @"<(\d+)>(\w+)<\1>");
            if (match.Success)
            {
                string outletNumber = match.Groups[1].Value;
                string value = match.Groups[2].Value;
                Console.WriteLine($"Outlet Number: {outletNumber}, Value: {value}");
                
                Logger.Log2($"result : {value}");
                return new Dictionary<string, string> { { outletNumber, value } };
            }
            else
            {
                Debug.WriteLine("패턴이 일치하지 않습니다.");
                Logger.LogError($"Error : 패턴이 일치하지 않습니다.");
                return new Dictionary<string, string> { { "Error", "Fail" } };
            }
         
        }
        catch (Exception e)
        {
            Logger.LogError($"PDU Error : {e.Message}");
            return new Dictionary<string, string> { { "Error", "Fail" } };
            
        }
    }



    public async Task<string> RebootAll(string url)
    {
        try
        {
            _baseUrl = $"http://{url}";
            return await PostAsync("/api/device/relay", new Dictionary<string, string>
        {
            {"method", "reboot"}
        });
        }catch(Exception e)
        {
            //Logger.Log2(e.Message);
            return "Fail";
        }
    }

    public async Task<string> OnPDUAll(string url)
    {
        try
        {
            _baseUrl = $"http://{url}";
            return await PostAsync("/api/device/relay", new Dictionary<string, string>
        {
            {"method", "on_immediate"}
        });
        }catch(Exception e)
        {
            //Logger.Log2(e.Message);
            return "Fail";
        }
    }

    public async Task<string> OnPDU(string url,string index)
    {
        try
        {
            _baseUrl = $"http://{url}";
            return await PostAsync("/api/outlet/relay", new Dictionary<string, string>
        {
            {"index", index},
            {"method", "on_immediate"}
        });
        }
        catch (Exception e)
        {
            //Logger.Log2(e.Message);
            return "Fail";
        }
    }

    public async Task<string> RebootPDU(string url,string index)
    {
        try
        {
            _baseUrl = $"http://{url}";
            return await PostAsync("/api/outlet/relay", new Dictionary<string, string>
        {
            {"index", index},
            {"method", "reboot"}

        });
        }
        catch (Exception e)
        {
            //Logger.Log2(e.Message);
            return "Fail";
        }
    }


    public async Task<string> OffPDU(string url, string index)
    {
        try
        {
            _baseUrl = $"http://{url}";
            return await PostAsync("/api/outlet/relay", new Dictionary<string, string>
        {
            {"index", index},
            {"method", "off_immediate"}

        });
        }
        catch (Exception e)
        {
            //Logger.Log2(e.Message);
            return "Fail";
        }
    }


    public async Task<string> OffPDUAll(string url)
    {
        try
        {
            _baseUrl = $"http://{url}";
            return await PostAsync("/api/device/relay", new Dictionary<string, string>
        {
            {"method", "off_immediate"}
        });
        }catch(Exception e)
        {
            //Logger.Log2(e.Message);
            return "Fail";
        }
    }
}