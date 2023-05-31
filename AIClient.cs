using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Diagnostics;

namespace GPT
{
    public class AI_Message
    {
        public string role { get; set; }
        public string content { get; set; }
    }

    public class AI_Body
    {
        public string model { get; set; }
        public int max_tokens { get; set; }
        public AI_Message[] messages { get; set; }
    }

    public class AI_Choice
    {
        public string finish_reason { get; set; }
        public int index { get; set; }
        public AI_Message message { get; set; }
    }

    public class AI_Usage
    {
        public int prompt_tokens { get; set; }
        public int completion_tokens { get; set; }
        public int total_tokens { get; set; }
    }

    public class AI_ChatCompletionResponse
    {
        public string id { get; set; }
        public long created { get; set; }
        public string model { get; set; }
        public string @object { get; set; }
        public List<AI_Choice> choices { get; set; }
        public AI_Usage usage { get; set; }
    }


    public class AIClient
    {
        private readonly string api_key;
        HttpClient client = new HttpClient();
        public AIClient(string api_key)
        {
            this.api_key = api_key;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://api.pawan.krd/resetip");
            request.Headers.Add("Authorization", $"Bearer {this.api_key}");
            client.SendAsync(request);
        }

        private string SerializeJson(AI_Body body)
        {
            string json = JsonConvert.SerializeObject(body);
            return json;
        }

        private async Task<AI_ChatCompletionResponse> ParseChatCompletion(Task<string> json)
        {
            AI_ChatCompletionResponse response = JsonConvert.DeserializeObject<AI_ChatCompletionResponse>(await json);
            return response;
        }

        public async Task<AI_ChatCompletionResponse> ChatCompletion(AI_Message[] messages)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://api.pawan.krd/v1/chat/completions");
            request.Headers.Add("Authorization", $"Bearer {this.api_key}");
            AI_Body body = new AI_Body
            {
                model = "gpt-3.5-turbo",
                max_tokens = 3800,
                messages = messages,
            };

            request.Content = new StringContent(SerializeJson(body));
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            HttpResponseMessage response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return await ParseChatCompletion(response.Content.ReadAsStringAsync());
            } else
            {
                Debug.WriteLine(response.Content.ReadAsStringAsync());
                return null;
            }
        }
    }
}