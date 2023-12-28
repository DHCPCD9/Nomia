using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nomia.Entities;
using Nomia.Exceptions;

namespace Nomia;

public class NomiaNodeRest
{

    private NomiaNode _node;
    private HttpClient _httpClient;
    
    public NomiaNodeRest(NomiaNode node)
    {
        _node = node;
        _httpClient = new HttpClient();
        
        prepareHttpClient();
    }

    private void prepareHttpClient()
    {
        _httpClient.BaseAddress = _node.RestEndpoint.ToUri();
        _httpClient.DefaultRequestHeaders.Add("Authorization", _node.RestEndpoint.Password);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "DHCPCD9/Nomia");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    #region Track resolvers
    public async Task<LavalinkLoadable> ResolveTracks(string query)
    {
        var builder = new UriBuilder(new Uri(string.Format(LavalinkEndpoints.TRACK_RESOLVE, _httpClient.BaseAddress)));
        var queryBuilder = HttpUtility.ParseQueryString(builder.Query);
        
        queryBuilder["identifier"] = query;
        
        builder.Query = queryBuilder.ToString();
        using var req = new HttpRequestMessage
        {
            Method = new HttpMethod("GET"),
            RequestUri = new Uri(builder.ToString())
        };
        
        using var res = await _httpClient.SendAsync(req);
        
        if (!res.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to resolve tracks: {res.StatusCode}");
        }
        
        var json = await res.Content.ReadAsStringAsync();
        
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new Exception("Failed to resolve tracks: empty response");
        }

        var responseObject = JsonConvert.DeserializeObject<JObject>(json);
        var loadType = responseObject.GetValue("loadType")?.ToObject<LavalinkLoadType>();

        return loadType switch
        {
            LavalinkLoadType.NoMatches => responseObject.ToObject<LavalinkEmptyLoadType>(),
            LavalinkLoadType.LoadFailed => responseObject.ToObject<LavalinkLoadFailedType>(),
            LavalinkLoadType.TrackLoaded => responseObject.ToObject<LavalinkTrackLoadedType>(),
            LavalinkLoadType.SearchResult => responseObject.ToObject<LavalinkSearchLoadedType>(),
            LavalinkLoadType.PlaylistLoaded => responseObject.ToObject<LavalinkPlaylistLoadedType>(),
            _ => responseObject.ToObject<LavalinkEmptyLoadType>() 
        };
    }

    #endregion

    #region Player

    internal async Task<LavalinkPlayer> UpdatePlayer(ulong guildId, LavalinkPlayerUpdatePayload payload, bool noReplace = false)
    {
        var builder = new UriBuilder(new Uri(string.Format(LavalinkEndpoints.PLAYER, _httpClient.BaseAddress, _node.SessionId, guildId)));
        var query = HttpUtility.ParseQueryString(builder.Query);
        if (noReplace)
        {
            query["noReplace"] = "true";
        }
        
        builder.Query = query.ToString();
        
        using var req = new HttpRequestMessage
        {
            Method = new HttpMethod("PATCH"),
            RequestUri = new Uri(builder.ToString())
        };
        


        using var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

        req.Content = content;
        
        using var res = await _httpClient.SendAsync(req);
        
        if (!res.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to update player: {res.StatusCode}");
        }
        
        var json = await res.Content.ReadAsStringAsync();
        
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new Exception("Failed to update player: No response");
        }

        return JsonConvert.DeserializeObject<LavalinkPlayer>(json);
    }
    
    internal async Task DestroyPlayer(ulong guildId)
    {
        var builder = new UriBuilder(new Uri(string.Format(LavalinkEndpoints.PLAYER, _httpClient.BaseAddress, _node.SessionId, guildId)));
        var query = HttpUtility.ParseQueryString(builder.Query);
        
        using var req = new HttpRequestMessage
        {
            Method = new HttpMethod("DELETE"),
            RequestUri = new Uri(builder.ToString())
        };
        
        using var res = await _httpClient.SendAsync(req);
        
        if (!res.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to destroy player: {res.StatusCode}");
        }
    }
    
    #endregion


}