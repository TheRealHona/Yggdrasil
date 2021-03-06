﻿using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Yggdrasil.Models;

namespace Yggdrasil
{
    public class YggdrasilService
    {
        private readonly HttpClient _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://authserver.mojang.com")
        };

        private async Task<YggdrasilResponse<TResponse>> PostAsync<TResponse, TPayload>(string endpoint, TPayload payload)
        {
            var response =
                await _httpClient.PostAsync(endpoint, new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));
            object stringValue = await response.Content.ReadAsStringAsync();

            var output = new YggdrasilResponse<TResponse>();
            if (response.IsSuccessStatusCode)
            {
                output.ErrorResponse = null;
                output.Success = true;

                // Deserializes and sets the response object
                // If T is a string, don't deserialize
                if (typeof(TResponse) == typeof(string))
                    output.Response = (TResponse) stringValue;
                else if (typeof(TResponse).IsEnum)
                    // Sets the enum responses to 1 (success/valid), can't do this cleanly since T may also be a class
                    output.Response = (TResponse) Enum.Parse(typeof(TResponse), "1");
                else
                    output.Response = JsonConvert.DeserializeObject<TResponse>((string)stringValue);
            }
            else
            {
                output.Success = false;

                // Request failed, response is now the error
                output.ErrorResponse = JsonConvert.DeserializeObject<ErrorResponse>((string) stringValue);

                // Setting the default value on failure also sets the Response enums to 0 (failure)
                output.Response = default;
            }

            return output;
        }

        public async Task<YggdrasilResponse<AuthenticateResponse>> AuthenticateAsync(AuthenticatePayload payload)
            => await PostAsync<AuthenticateResponse, AuthenticatePayload>("/authenticate", payload);

        public async Task<YggdrasilResponse<RefreshResponse>> RefreshAsync(RefreshPayload payload)
            => await PostAsync<RefreshResponse, RefreshPayload>("/refresh", payload);

        public async Task<YggdrasilResponse<ValidateResponse>> ValidateAsync(ValidatePayload payload)
            => await PostAsync<ValidateResponse, ValidatePayload>("/validate", payload);

        public async Task<YggdrasilResponse<StandardResponse>> SignoutAsync(SignoutPayload payload)
            => await PostAsync<StandardResponse, SignoutPayload>("/signout", payload);

        public async Task<YggdrasilResponse<StandardResponse>> InvalidateAsync(InvalidatePayload payload)
            => await PostAsync<StandardResponse, InvalidatePayload>("/invalidate", payload);
    }
}