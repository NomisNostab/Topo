﻿using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Text;
using Topo.Models.Login;
using Topo.Models.MemberList;
using Topo.Models.Events;
using Topo.Models.OAS;

namespace Topo.Services
{

    public interface ITerrainAPIService
    {
        public Task<AuthenticationResultModel?> LoginAsync(string? username, string? password);
        public Task<GetUserResultModel?> GetUserAsync();
        public Task GetProfilesAsync();
        public List<SelectListItem>? GetUnits();
        public Task RefreshTokenAsync();
        public Task<GetMembersResultModel?> GetMembersAsync();
        public Task<GetEventsResultModel?> GetEventsAsync(DateTime fromDate, DateTime toDate);
        public Task<GetEventResultModel?> GetEventAsync(string eventId);
        public Task<GetCalendarsResultModel?> GetCalendarsAsync();
        public Task<bool> PutCalendarsAsync(GetCalendarsResultModel putCalendarsResultModel);
        public Task<GetOASTreeResultsModel?> GetOASTreeAsync(string stream);
        public Task<GetUnitAchievementsResultsModel> GetUnitAchievements(string unit, string stream, string branch, int stage);
    }
    public class TerrainAPIService : ITerrainAPIService
    {
        private readonly StorageService _storageService;
        private readonly string cognitoAddress = "https://cognito-idp.ap-southeast-2.amazonaws.com/";
        private readonly string membersAddress = "https://members.terrain.scouts.com.au/";
        private readonly string eventsAddress = "https://events.terrain.scouts.com.au/";
        private readonly string templatesAddress = "https://templates.terrain.scouts.com.au/";
        private readonly string achievementsAddress = "https://achievements.terrain.scouts.com.au/";
        private readonly string clientId = "6v98tbc09aqfvh52fml3usas3c";

        public TerrainAPIService(StorageService storageService)
        {
            _storageService = storageService;
        }

        public async Task<AuthenticationResultModel?> LoginAsync(string? username, string? password)
        {

            AuthenticationResultModel? authenticationResult = new AuthenticationResultModel();
            using (var httpClient = new HttpClient())
            {
                var initiateAuth = new InitiateAuthModel();
                initiateAuth.ClientMetadata = new ClientMetadata();
                initiateAuth.AuthFlow = "USER_PASSWORD_AUTH";
                initiateAuth.ClientId = clientId;
                initiateAuth.AuthParameters = new AuthParameters();
                initiateAuth.AuthParameters.USERNAME = $"nsw-{username}";
                initiateAuth.AuthParameters.PASSWORD = password;

                HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, cognitoAddress);
                httpRequest.Content = new StringContent(JsonConvert.SerializeObject(initiateAuth), Encoding.UTF8, "application/x-amz-json-1.1");
                httpRequest.Headers.Add("X-Amz-Target", "AWSCognitoIdentityProviderService.InitiateAuth");
                httpRequest.Headers.Add("X-Amz-User-Agent", "aws-amplify/0.1.x js");

                var response = await httpClient.SendAsync(httpRequest);

                var responseContent = response.Content.ReadAsStringAsync();
                var result = responseContent.Result;
                authenticationResult = JsonConvert.DeserializeObject<AuthenticationResultModel>(result);
                _storageService.AuthenticationResult = authenticationResult?.AuthenticationResult;
                _storageService.IsAuthenticated = false;
                if (authenticationResult != null)
                {
                    _storageService.IsAuthenticated = true;
                    _storageService.TokenExpiry = DateTime.Now.AddSeconds((authenticationResult?.AuthenticationResult?.ExpiresIn ?? 0) - 60);
                }
            }
            return authenticationResult;
        }

        public async Task<GetUserResultModel?> GetUserAsync()
        {
            GetUserResultModel? getUserResultModel = new GetUserResultModel();
            using (var httpClient = new HttpClient())
            {
                HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, cognitoAddress);

                AccessTokenModel accessToken = new AccessTokenModel() { AccessToken = _storageService.AuthenticationResult?.AccessToken };

                httpRequest.Content = new StringContent(JsonConvert.SerializeObject(accessToken), Encoding.UTF8, "application/x-amz-json-1.1");
                httpRequest.Headers.Add("X-Amz-Target", "AWSCognitoIdentityProviderService.GetUser");
                httpRequest.Headers.Add("X-Amz-User-Agent", "aws-amplify/0.1.x js");

                var response = await httpClient.SendAsync(httpRequest);
                var responseContent = response.Content.ReadAsStringAsync();
                var result = responseContent.Result;
                getUserResultModel = JsonConvert.DeserializeObject<GetUserResultModel>(result);
                _storageService.GetUserResult = getUserResultModel;
            }
            return getUserResultModel;
        }

        public async Task RefreshTokenAsync()
        {
            if (_storageService.TokenExpiry < DateTime.Now)
            {
                AuthenticationResultModel? authenticationResult = new AuthenticationResultModel();
                using (var httpClient = new HttpClient())
                {
                    var initiateAuth = new InitiateAuthModel();
                    initiateAuth.ClientMetadata = new ClientMetadata();
                    initiateAuth.AuthFlow = "REFRESH_TOKEN_AUTH";
                    initiateAuth.ClientId = clientId;
                    initiateAuth.AuthParameters = new AuthParameters();
                    initiateAuth.AuthParameters.REFRESH_TOKEN = _storageService?.AuthenticationResult?.RefreshToken;
                    initiateAuth.AuthParameters.DEVICE_KEY = null;

                    HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, cognitoAddress);
                    httpRequest.Content = new StringContent(JsonConvert.SerializeObject(initiateAuth), Encoding.UTF8, "application/x-amz-json-1.1");
                    httpRequest.Headers.Add("X-Amz-Target", "AWSCognitoIdentityProviderService.InitiateAuth");
                    httpRequest.Headers.Add("X-Amz-User-Agent", "aws-amplify/0.1.x js");

                    var response = await httpClient.SendAsync(httpRequest);

                    var responseContent = response.Content.ReadAsStringAsync();
                    var result = responseContent.Result;
                    authenticationResult = JsonConvert.DeserializeObject<AuthenticationResultModel>(result);
                    if (_storageService != null && _storageService.AuthenticationResult != null)
                    {
                        _storageService.AuthenticationResult.AccessToken = authenticationResult?.AuthenticationResult?.AccessToken;
                        _storageService.AuthenticationResult.IdToken = authenticationResult?.AuthenticationResult?.IdToken;
                        _storageService.AuthenticationResult.ExpiresIn = authenticationResult?.AuthenticationResult?.ExpiresIn;
                        _storageService.AuthenticationResult.TokenType = authenticationResult?.AuthenticationResult?.TokenType;
                        _storageService.TokenExpiry = DateTime.Now.AddSeconds((authenticationResult?.AuthenticationResult?.ExpiresIn ?? 0) - 60);
                    }
                }
            }
        }

        public async Task GetProfilesAsync()
        {
            await RefreshTokenAsync();
            GetProfilesResultModel? getProfilesResultModel = new GetProfilesResultModel();
            using (var httpClient = new HttpClient())
            {
                HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, membersAddress + "profiles");

                httpRequest.Content = new StringContent("", Encoding.UTF8, "application/x-amz-json-1.1");
                httpRequest.Headers.Add("authorization", _storageService?.AuthenticationResult?.IdToken);
                httpRequest.Headers.Add("accept", "application/json, text/plain, */*");

                var response = await httpClient.SendAsync(httpRequest);
                var responseContent = response.Content.ReadAsStringAsync();
                var result = responseContent.Result;
                getProfilesResultModel = JsonConvert.DeserializeObject<GetProfilesResultModel>(result);
                if (_storageService != null)
                    _storageService.GetProfilesResult = getProfilesResultModel;
            }
        }

        public List<SelectListItem>? GetUnits()
        {
            return _storageService.GetProfilesResult?.profiles?.Select(p => p.unit).Distinct().Select(u => new SelectListItem { Text = u?.name, Value = u?.id }).ToList();
        }

        public async Task<GetMembersResultModel?> GetMembersAsync()
        {
            await RefreshTokenAsync();
            using (var httpClient = new HttpClient())
            {
                HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, $"{membersAddress}units/{_storageService.SelectedUnitId}/members");

                httpRequest.Content = new StringContent("", Encoding.UTF8, "application/x-amz-json-1.1");
                httpRequest.Headers.Add("authorization", _storageService?.AuthenticationResult?.IdToken);
                httpRequest.Headers.Add("accept", "application/json, text/plain, */*");

                var response = await httpClient.SendAsync(httpRequest);
                var responseContent = response.Content.ReadAsStringAsync();
                var result = responseContent.Result;
                var getMembersResultModel = JsonConvert.DeserializeObject<GetMembersResultModel>(result);

                return getMembersResultModel;
            }
        }

        public async Task<GetEventsResultModel?> GetEventsAsync(DateTime fromDate, DateTime toDate)
        {
            await RefreshTokenAsync();
            using (var httpClient = new HttpClient())
            {
                var userId = "";
                if (_storageService.GetProfilesResult != null && _storageService.GetProfilesResult.profiles != null && _storageService.GetProfilesResult.profiles.Length > 0)
                {
                    userId = _storageService.GetProfilesResult.profiles[0].member?.id;
                }
                var fromDateString = fromDate.ToString("s");
                var toDateString = toDate.ToString("s");
                HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, $"{eventsAddress}members/{userId}/events?start_datetime={fromDateString}&end_datetime={toDateString}");

                httpRequest.Content = new StringContent("", Encoding.UTF8, "application/x-amz-json-1.1");
                httpRequest.Headers.Add("authorization", _storageService?.AuthenticationResult?.IdToken);
                httpRequest.Headers.Add("accept", "application/json, text/plain, */*");

                var response = await httpClient.SendAsync(httpRequest);
                var responseContent = response.Content.ReadAsStringAsync();
                var result = responseContent.Result;
                var getEventsResultModel = JsonConvert.DeserializeObject<GetEventsResultModel>(result);

                return getEventsResultModel;
            }
        }

        public async Task<GetEventResultModel?> GetEventAsync(string eventId)
        {
            await RefreshTokenAsync();
            using (var httpClient = new HttpClient())
            {
                HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, $"{eventsAddress}events/{eventId}");

                httpRequest.Content = new StringContent("", Encoding.UTF8, "application/x-amz-json-1.1");
                httpRequest.Headers.Add("authorization", _storageService?.AuthenticationResult?.IdToken);
                httpRequest.Headers.Add("accept", "application/json, text/plain, */*");

                var response = await httpClient.SendAsync(httpRequest);
                var responseContent = response.Content.ReadAsStringAsync();
                var result = responseContent.Result;
                var getEventResultModel = JsonConvert.DeserializeObject<GetEventResultModel>(result);

                return getEventResultModel;
            }

        }

        public async Task<GetCalendarsResultModel?> GetCalendarsAsync()
        {
            await RefreshTokenAsync();
            using (var httpClient = new HttpClient())
            {
                var userId = "";
                if (_storageService.GetProfilesResult != null && _storageService.GetProfilesResult.profiles != null && _storageService.GetProfilesResult.profiles.Length > 0)
                {
                    userId = _storageService.GetProfilesResult.profiles[0].member?.id;
                }
                HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, $"{eventsAddress}members/{userId}/calendars");

                httpRequest.Content = new StringContent("", Encoding.UTF8, "application/x-amz-json-1.1");
                httpRequest.Headers.Add("authorization", _storageService?.AuthenticationResult?.IdToken);
                httpRequest.Headers.Add("accept", "application/json, text/plain, */*");

                var response = await httpClient.SendAsync(httpRequest);
                var responseContent = response.Content.ReadAsStringAsync();
                var result = responseContent.Result;
                var getCalendarsResultModel = JsonConvert.DeserializeObject<GetCalendarsResultModel>(result);

                return getCalendarsResultModel;
            }
        }

        public async Task<bool> PutCalendarsAsync(GetCalendarsResultModel putCalendarsResultModel)
        {
            await RefreshTokenAsync();
            using (var httpClient = new HttpClient())
            {
                var userId = "";
                if (_storageService.GetProfilesResult != null && _storageService.GetProfilesResult.profiles != null && _storageService.GetProfilesResult.profiles.Length > 0)
                {
                    userId = _storageService.GetProfilesResult.profiles[0].member?.id;
                }
                HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Put, $"{eventsAddress}members/{userId}/calendars");
                httpRequest.Content = new StringContent(JsonConvert.SerializeObject(putCalendarsResultModel), Encoding.UTF8, "application/json");
                httpRequest.Headers.Add("authorization", _storageService?.AuthenticationResult?.IdToken);
                httpRequest.Headers.Add("accept", "application/json, text/plain, */*");

                var response = await httpClient.SendAsync(httpRequest);
                var responseContent = response.Content.ReadAsStringAsync();
                var result = responseContent.Result;
                return true;
                
            }
        }

        public async Task<GetOASTreeResultsModel?> GetOASTreeAsync(string stream)
        {
            await RefreshTokenAsync();
            using (var httpClient = new HttpClient())
            {
                HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, $"{templatesAddress}oas/{stream}/tree.json");

                httpRequest.Content = new StringContent("", Encoding.UTF8, "application/x-amz-json-1.1");
                httpRequest.Headers.Add("authorization", _storageService?.AuthenticationResult?.IdToken);
                httpRequest.Headers.Add("accept", "application/json, text/plain, */*");

                var response = await httpClient.SendAsync(httpRequest);
                var responseContent = response.Content.ReadAsStringAsync();
                var result = responseContent.Result;
                var getOASTreeResultsModel = JsonConvert.DeserializeObject<GetOASTreeResultsModel>(result);

                return getOASTreeResultsModel;
            }
        }

        public async Task<GetUnitAchievementsResultsModel> GetUnitAchievements(string unit, string stream, string branch, int stage)
        {
            await RefreshTokenAsync();
            using (var httpClient = new HttpClient())
            {
                HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, $"{achievementsAddress}units/{unit}/achievements?type=outdoor_adventure_skill&stream={stream}&branch={branch}&stage={stage}");

                httpRequest.Content = new StringContent("", Encoding.UTF8, "application/x-amz-json-1.1");
                httpRequest.Headers.Add("authorization", _storageService?.AuthenticationResult?.IdToken);
                httpRequest.Headers.Add("accept", "application/json, text/plain, */*");

                var response = await httpClient.SendAsync(httpRequest);
                var responseContent = response.Content.ReadAsStringAsync();
                var result = responseContent.Result;
                var getUnitAchievementsResultsModel = JsonConvert.DeserializeObject<GetUnitAchievementsResultsModel>(result);

                return getUnitAchievementsResultsModel;
            }
        }
    }
}
