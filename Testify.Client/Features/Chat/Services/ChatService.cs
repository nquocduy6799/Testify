using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Testify.Client.Interfaces;
using Testify.Shared.DTOs.Chat;

namespace Testify.Client.Features.Chat.Services
{
    public class ChatService : IChatService
    {
        private readonly HttpClient _httpClient;
        private const string ApiEndpoint = "api/chat";

        public ChatService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        #region Rooms

        public async Task<List<ChatRoomResponse>> GetChatRoomsAsync()
        {
            try
            {
                var rooms = await _httpClient.GetFromJsonAsync<List<ChatRoomResponse>>($"{ApiEndpoint}/rooms");
                return rooms ?? new List<ChatRoomResponse>();
            }
            catch (HttpRequestException) { return new List<ChatRoomResponse>(); }
        }

        public async Task<ChatRoomResponse?> GetChatRoomByIdAsync(int roomId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<ChatRoomResponse>($"{ApiEndpoint}/rooms/{roomId}");
            }
            catch (HttpRequestException) { return null; }
        }

        public async Task<ChatRoomResponse> CreateChatRoomAsync(CreateChatRoomRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{ApiEndpoint}/rooms", request);
                response.EnsureSuccessStatusCode();
                
                var room = await response.Content.ReadFromJsonAsync<ChatRoomResponse>();
                return room ?? throw new InvalidOperationException("Failed to create chat room");
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to create chat room: {ex.Message}", ex);
            }
        }

        public async Task<ChatRoomResponse?> GetOrCreateDirectRoomAsync(string otherUserId)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{ApiEndpoint}/rooms/direct", otherUserId);
                response.EnsureSuccessStatusCode();
                
                return await response.Content.ReadFromJsonAsync<ChatRoomResponse>();
            }
            catch (HttpRequestException) { return null; }
        }

        public async Task<List<ChatUserItemResponse>> GetUsersForChatAsync(string? search = null)
        {
            try
            {
                var url = $"{ApiEndpoint}/users";
                if (!string.IsNullOrWhiteSpace(search))
                    url += $"?search={Uri.EscapeDataString(search.Trim())}";
                var list = await _httpClient.GetFromJsonAsync<List<ChatUserItemResponse>>(url);
                return list ?? new List<ChatUserItemResponse>();
            }
            catch (HttpRequestException) { return new List<ChatUserItemResponse>(); }
        }

        public async Task<string?> GetCurrentUserIdAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ChatCurrentUserResponse>($"{ApiEndpoint}/me");
                return response?.UserId;
            }
            catch (HttpRequestException) { return null; }
        }

        #endregion

        #region Messages

        public async Task<List<ChatMessageResponse>> GetRoomMessagesAsync(int roomId, int skip = 0, int take = 50)
        {
            try
            {
                var messages = await _httpClient.GetFromJsonAsync<List<ChatMessageResponse>>(
                    $"{ApiEndpoint}/rooms/{roomId}/messages?skip={skip}&take={take}");
                return messages ?? new List<ChatMessageResponse>();
            }
            catch (HttpRequestException) { return new List<ChatMessageResponse>(); }
        }

        public async Task<ChatMessageResponse> SendMessageAsync(SendMessageRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{ApiEndpoint}/messages", request);
                response.EnsureSuccessStatusCode();
                
                var message = await response.Content.ReadFromJsonAsync<ChatMessageResponse>();
                return message ?? throw new InvalidOperationException("Failed to send message");
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to send message: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteMessageAsync(int messageId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{ApiEndpoint}/messages/{messageId}");
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException) { return false; }
        }

        public async Task<ChatMessageResponse> UpdateMessageAsync(int messageId, string newContent)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{ApiEndpoint}/messages/{messageId}", newContent);
                response.EnsureSuccessStatusCode();
                
                var message = await response.Content.ReadFromJsonAsync<ChatMessageResponse>();
                return message ?? throw new InvalidOperationException("Failed to update message");
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to update message: {ex.Message}", ex);
            }
        }

        #endregion

        #region Reactions

        public async Task<bool> AddReactionAsync(AddReactionRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{ApiEndpoint}/reactions", request);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException) { return false; }
        }

        public async Task<bool> RemoveReactionAsync(int messageId, string emoji)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{ApiEndpoint}/reactions/{messageId}?emoji={Uri.EscapeDataString(emoji)}");
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException) { return false; }
        }

        #endregion

        #region Read Receipts

        public async Task<bool> MarkAsReadAsync(int roomId, int messageId)
        {
            try
            {
                var request = new MarkAsReadRequest { RoomId = roomId, MessageId = messageId };
                var response = await _httpClient.PostAsJsonAsync($"{ApiEndpoint}/read", request);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException) { return false; }
        }

        public async Task<int> GetUnreadCountAsync(int roomId)
        {
            try
            {
                var count = await _httpClient.GetFromJsonAsync<int>($"{ApiEndpoint}/rooms/{roomId}/unread");
                return count;
            }
            catch (HttpRequestException) { return 0; }
        }

        #endregion
    }
}
