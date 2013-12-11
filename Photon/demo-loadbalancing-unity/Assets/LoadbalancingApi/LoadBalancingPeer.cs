// ----------------------------------------------------------------------------
// <copyright file="LoadBalancingPeer.cs" company="Exit Games GmbH">
//   Loadbalancing Framework for Photon - Copyright (C) 2011 Exit Games GmbH
// </copyright>
// <summary>
//   Provides the operations needed to use the "loadbalancing" server
//   application which is also used in Photon Cloud.
//   No logic is implemented here.
// </summary>
// <author>developer@exitgames.com</author>
// ----------------------------------------------------------------------------

namespace ExitGames.Client.Photon.LoadBalancing
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using ExitGames.Client.Photon;
    using ExitGames.Client.Photon.Lite;

#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_DASHBOARD_WIDGET || UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_WII || UNITY_IPHONE || UNITY_ANDROID || UNITY_PS3 || UNITY_XBOX360 || UNITY_NACL  || UNITY_FLASH  || UNITY_BLACKBERRY
    using Hashtable = ExitGames.Client.Photon.Hashtable;
#endif

    /// <summary>
    /// A LoadbalancingPeer provides the operations and enum definitions needed to use the loadbalancing server application which is also used in Photon Cloud.
    /// </summary>
    /// <remarks>
    /// The LoadBalancingPeer does not keep a state, instead this is done by a LoadBalancingClient.
    /// </remarks>
    public class LoadBalancingPeer : PhotonPeer
    {
        /// <summary>
        /// Creates a Peer with selected connection protocol.
        /// </summary>
        /// <remarks>Each connection protocol has it's own default networking ports for Photon.</remarks>
        /// <param name="protocolType">The preferred option is UDP.</param>
        public LoadBalancingPeer(ConnectionProtocol protocolType) : base(protocolType)
        {
            // this does not require a Listener, so:
            // make sure to set this.Listener before using a peer!
        }

        /// <summary>
        /// Creates a Peer with default connection protocol (UDP).
        /// </summary>
        public LoadBalancingPeer(IPhotonPeerListener listener, ConnectionProtocol protocolType) : base(listener, protocolType)
        {
        }

        /// <summary>
        /// Calls OpJoinLobby(string name, LobbyType lobbyType).
        /// </summary>
        /// <returns>If the operation could be sent (requires connection).</returns>
        public virtual bool OpJoinLobby()
        {
            return this.OpJoinLobby(null, LobbyType.Default);
        }

        /// <summary>
        /// Joins the lobby on the Master Server, where you get a list of RoomInfos of currently open rooms.
        /// This is an async request which triggers a OnOperationResponse() call.
        /// </summary>
        /// <returns>If the operation could be sent (has to be connected).</returns>
        public virtual bool OpJoinLobby(string name, LobbyType lobbyType)
        {
            Dictionary<byte, object> parameters = new Dictionary<byte, object>();
            parameters[(byte)ParameterCode.LobbyName] = name;
            parameters[(byte)ParameterCode.LobbyType] = (byte)lobbyType;

            return this.OpCustom(OperationCode.JoinLobby, parameters, true);
        }

        /// <summary>
        /// Leaves the lobby on the Master Server.
        /// This is an async request which triggers a OnOperationResponse() call.
        /// </summary>
        /// <returns>If the operation could be sent (requires connection).</returns>
        public virtual bool OpLeaveLobby()
        {
            if (this.DebugOut >= DebugLevel.INFO)
            {
                this.Listener.DebugReturn(DebugLevel.INFO, "OpLeaveLobby()");
            }

            return this.OpCustom(OperationCode.LeaveLobby, null, true);
        }

        /// <summary>
        /// Creates a room (on either Master or Game Server).
        /// The OperationResponse depends on the server the peer is connected to:
        /// Master will return a Game Server to connect to.
        /// Game Server will return the joined Room's data.
        /// This is an async request which triggers a OnOperationResponse() call.
        /// </summary>
        /// <remarks>
        /// If the room is already existing, the OperationResponse will have a returnCode of ErrorCode.GameAlreadyExists.
        /// </remarks>
        /// <param name="roomName">The name of this room. Must be unique. Pass null to make the server assign a name.</param>
        /// <param name="isVisible">Defines if this room is listed in the lobby. If not, it also is not joined randomly.</param>
        /// <param name="isOpen">Defines if this room can be joined at all.</param>
        /// <param name="maxPlayers">Max number of players that can be in the room at any time. 0 means "no limit".</param>
        /// <param name="customGameProperties">The room's custom properties to set. Use string keys!</param>
        /// <param name="propsListedInLobby">Defines the custom room properties that get listed in the lobby. Null defaults to "none", a string[0].</param>
        /// <param name="playerProperties">This player's custom properties. Use string keys!</param>
        /// <param name="lobbyName">Name of the lobby this game gets added to. Default: null, attached to default lobby. Lobbies are unique per lobbyName plus lobbyType, so the same name can be used when several types are existing.</param>
        /// <param name="lobbyType">Type of the (named)lobby this game gets added to.</param>
        /// <returns>If the operation could be sent (requires connection).</returns>
        public virtual bool OpCreateRoom(string roomName, bool isVisible, bool isOpen, byte maxPlayers, Hashtable customGameProperties, string[] propsListedInLobby, Hashtable playerProperties, string lobbyName, LobbyType lobbyType)
        {
            if (this.DebugOut >= DebugLevel.INFO)
            {
                this.Listener.DebugReturn(DebugLevel.INFO, "OpCreateRoom()");
            }

            Hashtable gameProperties = new Hashtable();
            gameProperties[GameProperties.IsOpen] = isOpen;
            gameProperties[GameProperties.IsVisible] = isVisible;
            gameProperties[GameProperties.PropsListedInLobby] = (propsListedInLobby == null) ? new string[0] : propsListedInLobby;
            gameProperties.MergeStringKeys(customGameProperties);
            if (maxPlayers > 0)
            {
                gameProperties[GameProperties.MaxPlayers] = maxPlayers;
            }

            Dictionary<byte, object> op = new Dictionary<byte, object>();
            op[ParameterCode.GameProperties] = gameProperties;
            op[ParameterCode.CleanupCacheOnLeave] = true;   // Note: This is not yet optional. It makes sense to get this into the API when caching is explained, too
            op[ParameterCode.Broadcast] = true;
            if (playerProperties != null)
            {
                op[ParameterCode.PlayerProperties] = playerProperties;
            }

            if (!string.IsNullOrEmpty(roomName))
            {
                op[ParameterCode.RoomName] = roomName;
            }

            if (!string.IsNullOrEmpty(lobbyName))
            {
                op[ParameterCode.LobbyName] = lobbyName;
                op[ParameterCode.LobbyType] = lobbyType;
            }
            return this.OpCustom(OperationCode.CreateGame, op, true);
        }

        /// <summary>
        /// Joins a room by name and sets this player's properties (on either Master or Game Server).
        /// The OperationResponse depends on the server the peer is connected to:
        /// Master will return a Game Server to connect to.
        /// Game Server will return the joined Room's data.
        /// This is an async request which triggers a OnOperationResponse() call.
        /// </summary>
        /// <remarks>
        /// If the room is not existing (anymore), the OperationResponse will have a returnCode of ErrorCode.GameDoesNotExist.
        /// Other possible ErrorCodes are: GameClosed, GameFull.
        /// </remarks>
        /// <param name="roomName">The name of an existing room.</param>
        /// <param name="playerProperties">This player's custom properties.</param>
        /// <param name="actorId">To allow players to return to a game, they have to specify their actorId.</param>
        /// <param name="createIfNotExists">If true, the server will attempt to create a room, making the success callback OnCreatedRoom().</param>
        /// <returns>If the operation could be sent (requires connection).</returns>
        public virtual bool OpJoinRoom(string roomName, Hashtable playerProperties, int actorId, bool createIfNotExists)
        {
            if (this.DebugOut >= DebugLevel.INFO)
            {
                this.Listener.DebugReturn(DebugLevel.INFO, "OpJoinRoom()");
            }

            if (string.IsNullOrEmpty(roomName))
            {
                this.Listener.DebugReturn(DebugLevel.ERROR, "OpJoinRoom() failed. Please specify a roomname.");
                return false;
            }

            Dictionary<byte, object> op = new Dictionary<byte, object>();
            op[ParameterCode.RoomName] = roomName;
            op[ParameterCode.Broadcast] = true;

            if (createIfNotExists)
            {
                op[ParameterCode.CreateIfNotExists] = createIfNotExists;
            }

            if (actorId > 0)
            {
                op[ParameterCode.ActorNr] = actorId;
            }

            if (playerProperties != null)
            {
                op[ParameterCode.PlayerProperties] = playerProperties;
            }

            return this.OpCustom(OperationCode.JoinGame, op, true);
        }


        /// <summary>
        /// Joins a room by name and sets this player's properties (on either Master or Game Server).
        /// The OperationResponse depends on the server the peer is connected to:
        /// Master will return a Game Server to connect to.
        /// Game Server will return the joined Room's data.
        /// This is an async request which triggers a OnOperationResponse() call.
        /// </summary>
        /// <remarks>
        /// If the room is not existing (anymore), the OperationResponse will have a returnCode of ErrorCode.GameDoesNotExist.
        /// Other possible ErrorCodes are: GameClosed, GameFull.
        /// </remarks>
        /// <param name="roomName">The name of an existing room.</param>
        /// <param name="playerProperties">This player's custom properties.</param>
        /// <returns>If the operation could be sent (requires connection).</returns>
		public virtual bool OpJoinRoom(string roomName, Hashtable playerProperties)
		{
			return this.OpJoinRoom (roomName, playerProperties, 0, false);
		}

        /// <summary>
        /// Operation to join a random, available room. Overloads take additional player properties.
        /// This is an async request which triggers a OnOperationResponse() call.
        /// If all rooms are closed or full, the OperationResponse will have a returnCode of ErrorCode.NoRandomMatchFound.
        /// If successful, the OperationResponse contains a gameserver address and the name of some room.
        /// </summary>
        /// <param name="expectedCustomRoomProperties">Optional. A room will only be joined, if it matches these custom properties (with string keys).</param>
        /// <param name="expectedMaxPlayers">Filters for a particular maxplayer setting. Use 0 to accept any maxPlayer value.</param>
        /// <param name="playerProperties">This player's properties (custom and well known alike).</param>
        /// <param name="matchingType">Selects one of the available matchmaking algorithms. See MatchmakingMode enum for options.</param>
        /// <param name="lobbyName">Name of the lobby in which to find a room. Default: null.</param>
        /// <param name="lobbyType">Type of the (named)lobby this game gets added to.</param>
        /// <param name="sqlLobbyFilter">Basically the "where" clause of a sql statement. Use null for random game. Examples: "C0 = 1 AND C2 > 50". "C5 = \"Map2\" AND C2 > 10 AND C2 < 20"</param>
        /// <returns>If the operation could be sent currently (requires connection).</returns>
        public virtual bool OpJoinRandomRoom(Hashtable expectedCustomRoomProperties, byte expectedMaxPlayers, Hashtable playerProperties, MatchmakingMode matchingType, string lobbyName, LobbyType lobbyType, string sqlLobbyFilter)
        {
            if (this.DebugOut >= DebugLevel.INFO)
            {
                this.Listener.DebugReturn(DebugLevel.INFO, "OpJoinRandomRoom()");
            }

            Hashtable expectedRoomProperties = new Hashtable();
            expectedRoomProperties.MergeStringKeys(expectedCustomRoomProperties);
            if (expectedMaxPlayers > 0)
            {
                expectedRoomProperties[GameProperties.MaxPlayers] = expectedMaxPlayers;
            }

            Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
            if (expectedRoomProperties.Count > 0)
            {
                opParameters[ParameterCode.GameProperties] = expectedRoomProperties;
            }

            if (playerProperties != null && playerProperties.Count > 0)
            {
                opParameters[ParameterCode.PlayerProperties] = playerProperties;
            }

            if (matchingType != MatchmakingMode.FillRoom)
            {
                opParameters[ParameterCode.MatchMakingType] = (byte)matchingType;
            }

            if (!string.IsNullOrEmpty(lobbyName))
            {
                opParameters[ParameterCode.LobbyName] = lobbyName;
                opParameters[ParameterCode.LobbyType] = (byte)lobbyType;
            }

            if (!string.IsNullOrEmpty(sqlLobbyFilter))
            {
                opParameters[ParameterCode.Data] = sqlLobbyFilter;
            }
            return this.OpCustom(OperationCode.JoinRandomGame, opParameters, true);
        }

        /// <summary>
        /// Request the rooms and online status for a list of friends (each client must set a unique username via OpAuthenticate).
        /// </summary>
        /// <remarks>
        /// Used on Master Server to find the rooms played by a selected list of users.
        /// Users identify themselves by using OpAuthenticate with a unique username.
        /// The list of usernames must be fetched from some other source (not provided by Photon).
        ///
        /// The server response includes 2 arrays of info (each index matching a friend from the request):
        /// ParameterCode.FindFriendsResponseOnlineList = bool[] of online states
        /// ParameterCode.FindFriendsResponseRoomIdList = string[] of room names (empty string if not in a room)
        /// </remarks>
        /// <param name="friendsToFind">Array of friend's names (make sure they are unique).</param>
        /// <returns>If the operation could be sent (requires connection).</returns>
        public virtual bool OpFindFriends(string[] friendsToFind)
        {
            Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
            if (friendsToFind != null && friendsToFind.Length > 0)
            {
                opParameters[ParameterCode.FindFriendsRequestList] = friendsToFind;
            }

            return this.OpCustom(OperationCode.FindFriends, opParameters, true);
        }

        /// <summary>
        /// Sets custom properties of a player / actor (only passing on the string-typed custom properties).
        /// Internally this uses OpSetProperties, which can be used to either set room or player properties.
        /// </summary>
        /// <param name="actorNr">The player ID (a.k.a. actorNumber) of the player to attach these properties to.</param>
        /// <param name="actorProperties">The custom properties to add or update.</param>
        /// <returns>If the operation could be sent (requires connection).</returns>
        public bool OpSetCustomPropertiesOfActor(int actorNr, Hashtable actorProperties)
        {
            return this.OpSetPropertiesOfActor(actorNr, actorProperties.StripToStringKeys());
        }

        /// <summary>
        /// Sets properties of a player / actor.
        /// Internally this uses OpSetProperties, which can be used to either set room or player properties.
        /// </summary>
        /// <param name="actorNr">The payer ID (a.k.a. actorNumber) of the player to attach these properties to.</param>
        /// <param name="actorProperties">The properties to add or update.</param>
        /// <returns>If the operation could be sent (requires connection).</returns>
        public virtual bool OpSetPropertiesOfActor(int actorNr, Hashtable actorProperties)
        {
            if (this.DebugOut >= DebugLevel.INFO)
            {
                this.Listener.DebugReturn(DebugLevel.INFO, "OpSetPropertiesOfActor()");
            }

            Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
            opParameters.Add(ParameterCode.Properties, actorProperties);
            opParameters.Add(ParameterCode.ActorNr, actorNr);
            opParameters.Add(ParameterCode.Broadcast, true);

            return this.OpCustom((byte)OperationCode.SetProperties, opParameters, true, 0, false);
        }

        /// <summary>
        /// Set a "well known" property of a room.
        /// Internally this uses OpSetProperties, which can be used to either set room or player properties.
        /// </summary>
        /// <param name="propCode">The property name / key to set the given value for.</param>
        /// <param name="value">The property value to set for the room.</param>
        /// <returns>If the operation could be sent (requires connection).</returns>
        protected internal void OpSetPropertyOfRoom(byte propCode, object value)
        {
            Hashtable properties = new Hashtable();
            properties[propCode] = value;
            this.OpSetPropertiesOfRoom(properties);
        }

        /// <summary>
        /// Sets custom properties of a room (only passing string-typed keys in the Hashtable).
        /// Internally this uses OpSetProperties, which can be used to either set room or player properties.
        /// </summary>
        /// <param name="gameProperties"></param>
        /// <returns>If the operation could be sent (has to be connected).</returns>
        public bool OpSetCustomPropertiesOfRoom(Hashtable gameProperties)
        {
            return this.OpSetPropertiesOfRoom(gameProperties.StripToStringKeys());
        }

        /// <summary>
        /// Sets properties of a room.
        /// Internally this uses OpSetProperties, which can be used to either set room or player properties.
        /// </summary>
        /// <param name="gameProperties"></param>
        /// <returns>If the operation could be sent (has to be connected).</returns>
        protected internal virtual bool OpSetPropertiesOfRoom(Hashtable gameProperties)
        {
            if (this.DebugOut >= DebugLevel.INFO)
            {
                this.Listener.DebugReturn(DebugLevel.INFO, "OpSetPropertiesOfRoom()");
            }

            Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
            opParameters.Add(ParameterCode.Properties, gameProperties);
            opParameters.Add(ParameterCode.Broadcast, true);

            return this.OpCustom((byte)OperationCode.SetProperties, opParameters, true, 0, false);
        }

        /// <summary>
        /// Sends this app's appId and appVersion to identify this application server side.
        /// This is an async request which triggers a OnOperationResponse() call.
        /// </summary>
        /// <remarks>
        /// This operation makes use of encryption, if that is established before.
        /// See: EstablishEncryption(). Check encryption with IsEncryptionAvailable.
        /// This operation is allowed only once per connection (multiple calls will have ErrorCode != Ok).
        /// </remarks>
        /// <param name="appId">Your application's name or ID to authenticate. This is assigned by Photon Cloud (webpage).</param>
        /// <param name="appVersion">The client's version (clients with differing client appVersions are separated and players don't meet).</param>
        /// <returns>If the operation could be sent (has to be connected).</returns>
        [Obsolete("Use the other overload now.")]
        public virtual bool OpAuthenticate(string appId, string appVersion)
        {
            return OpAuthenticate(appId, appVersion, null);
        }

        /// <summary>
        /// Sends this app's appId and appVersion to identify this application server side.
        /// This is an async request which triggers a OnOperationResponse() call.
        /// </summary>
        /// <remarks>
        /// This operation makes use of encryption, if that is established before.
        /// See: EstablishEncryption(). Check encryption with IsEncryptionAvailable.
        /// This operation is allowed only once per connection (multiple calls will have ErrorCode != Ok).
        /// </remarks>
        /// <param name="appId">Your application's name or ID to authenticate. This is assigned by Photon Cloud (webpage).</param>
        /// <param name="appVersion">The client's version (clients with differing client appVersions are separated and players don't meet).</param>
        /// <param name="userId"></param>
        /// <returns>If the operation could be sent (has to be connected).</returns>
        public virtual bool OpAuthenticate(string appId, string appVersion, string userId)
        {
            return OpAuthenticate(appId, appVersion, userId, null);
        }

        /// <summary>
        /// Sends this app's appId and appVersion to identify this application server side.
        /// This is an async request which triggers a OnOperationResponse() call.
        /// </summary>
        /// <remarks>
        /// This operation makes use of encryption, if that is established before.
        /// See: EstablishEncryption(). Check encryption with IsEncryptionAvailable.
        /// This operation is allowed only once per connection (multiple calls will have ErrorCode != Ok).
        /// </remarks>
        /// <param name="appId">Your application's name or ID to authenticate. This is assigned by Photon Cloud (webpage).</param>
        /// <param name="appVersion">The client's version (clients with differing client appVersions are separated and players don't meet).</param>
        /// <param name="userId"></param>
        /// <param name="authValues"></param>
        /// <returns>If the operation could be sent (has to be connected).</returns>
        public virtual bool OpAuthenticate(string appId, string appVersion, string userId, AuthenticationValues authValues)
        {
            if (this.DebugOut >= DebugLevel.INFO)
            {
                this.Listener.DebugReturn(DebugLevel.INFO, "OpAuthenticate()");
            }

            Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
            opParameters[ParameterCode.AppVersion] = appVersion;
            opParameters[ParameterCode.ApplicationId] = appId;
            if (!string.IsNullOrEmpty(userId))
            {
                opParameters[ParameterCode.UserId] = userId;
            }

            if (authValues != null && authValues.AuthType != CustomAuthenticationType.None)
            {
                opParameters[ParameterCode.ClientAuthenticationType] = (byte)authValues.AuthType;
                if (!string.IsNullOrEmpty(authValues.Secret))
                {
                    opParameters[ParameterCode.Secret] = authValues.Secret;
                }
                else
                {
                    if (!string.IsNullOrEmpty(authValues.AuthParameters))
                    {
                        opParameters[ParameterCode.ClientAuthenticationParams] = authValues.AuthParameters;
                    }
                    if (authValues.AuthPostData != null)
                    {
                        opParameters[ParameterCode.ClientAuthenticationData] = authValues.AuthPostData;
                    }
                }
            }

            return this.OpCustom(OperationCode.Authenticate, opParameters, true, (byte)0, this.IsEncryptionAvailable);
        }

        /// <summary>
        /// Operation to handle this client's interest groups (for events in room).
        /// </summary>
        /// <remarks>
        /// Note the difference between passing null and byte[0]:
        ///   null won't add/remove any groups.
        ///   byte[0] will add/remove all (existing) groups.
        /// First, removing groups is executed. This way, you could leave all groups and join only the ones provided.
        ///
        /// Changes become active not immediately but when the server executes this operation (approximately RTT/2).
        /// </remarks>
        /// <param name="groupsToRemove">Groups to remove from interest. Null will not leave any. A byte[0] will remove all.</param>
        /// <param name="groupsToAdd">Groups to add to interest. Null will not add any. A byte[0] will add all current.</param>
        /// <returns></returns>
        public virtual bool OpChangeGroups(byte[] groupsToRemove, byte[] groupsToAdd)
        {
            if (this.DebugOut >= DebugLevel.ALL)
            {
                this.Listener.DebugReturn(DebugLevel.ALL, "OpChangeGroups()");
            }

            Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
            if (groupsToRemove != null)
            {
                opParameters[(byte)LiteOpKey.Remove] = groupsToRemove;
            }
            if (groupsToAdd != null)
            {
                opParameters[(byte)LiteOpKey.Add] = groupsToAdd;
            }

            return this.OpCustom((byte)LiteOpCode.ChangeGroups, opParameters, true, 0);
        }

        /// <summary>
        /// Send an event with custom code/type and any content to the other players in the same room.
        /// </summary>
        /// <remarks>This override explicitly uses another parameter order to not mix it up with the implementation for Hashtable only.</remarks>
        /// <param name="eventCode">Identifies this type of event (and the content). Your game's event codes can start with 0.</param>
        /// <param name="sendReliable">If this event has to arrive reliably (potentially repeated if it's lost).</param>
        /// <param name="customEventContent">Any serializable datatype (including Hashtable like the other OpRaiseEvent overloads).</param>
        /// <returns>If operation could be enqueued for sending. Sent when calling: Service or SendOutgoingCommands.</returns>
        public virtual bool OpRaiseEvent(byte eventCode, bool sendReliable, object customEventContent)
        {
            return this.OpRaiseEvent(eventCode, sendReliable, customEventContent, 0, EventCaching.DoNotCache, null, ReceiverGroup.Others, 0);
        }

        /// <summary>
        /// Send an event with custom code/type and any content to the other players in the same room.
        /// </summary>
        /// <remarks>This override explicitly uses another parameter order to not mix it up with the implementation for Hashtable only.</remarks>
        /// <param name="eventCode">Identifies this type of event (and the content). Your game's event codes can start with 0.</param>
        /// <param name="sendReliable">If this event has to arrive reliably (potentially repeated if it's lost).</param>
        /// <param name="customEventContent">Any serializable datatype (including Hashtable like the other OpRaiseEvent overloads).</param>
        /// <param name="channelId">Command sequence in which this command belongs. Must be less than value of ChannelCount property. Default: 0.</param>
        /// <param name="cache">Affects how the server will treat the event caching-wise. Can cache events for players joining later on or remove previously cached events. Default: DoNotCache.</param>
        /// <param name="targetActors">List of ActorNumbers (in this room) to send the event to. Overrides caching. Default: null.</param>
        /// <param name="receivers">Defines a target-player group. Default: Others.</param>
        /// <param name="interestGroup">Defines to which interest group the event is sent. Players can subscribe or unsibscribe to groups. Group 0 is always sent to all. Default: 0.</param>
        /// <returns>If operation could be enqueued for sending. Sent when calling: Service or SendOutgoingCommands.</returns>
        public virtual bool OpRaiseEvent(byte eventCode, bool sendReliable, object customEventContent, byte channelId, EventCaching cache, int[] targetActors, ReceiverGroup receivers, byte interestGroup)
        {
            Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
            opParameters[(byte)LiteOpKey.Code] = (byte)eventCode;

            if (customEventContent != null)
            {
                opParameters[(byte)LiteOpKey.Data] = customEventContent;
            }
            if (cache != EventCaching.DoNotCache)
            {
                opParameters[(byte)LiteOpKey.Cache] = (byte)cache;
            }
            if (receivers != ReceiverGroup.Others)
            {
                opParameters[(byte)LiteOpKey.ReceiverGroup] = (byte)receivers;
            }
            if (interestGroup != 0)
            {
                opParameters[(byte)LiteOpKey.Group] = (byte)interestGroup;
            }
            if (targetActors != null)
            {
                opParameters[(byte)LiteOpKey.ActorList] = targetActors;
            }

            return this.OpCustom((byte)LiteOpCode.RaiseEvent, opParameters, sendReliable, channelId, false);
        }

        /// <summary>
        /// Send your custom data as event to an "interest group" in the current Room.
        /// </summary>
        /// <remarks>
        /// No matter if reliable or not, when an event is sent to a interest Group, some users won't get this data.
        /// Clients can control the groups they are interested in by using OpChangeGroups.
        /// </remarks>
        /// <param name="eventCode">Identifies this type of event (and the content). Your game's event codes can start with 0.</param>
        /// <param name="interestGroup">The ID of the interest group this event goes to (exclusively).</param>
        /// <param name="customEventContent">Custom data you want to send along (use null, if none).</param>
        /// <param name="sendReliable">If this event has to arrive reliably (potentially repeated if it's lost).</param>
        /// <returns>If operation could be enqueued for sending</returns>
        public virtual bool OpRaiseEvent(byte eventCode, byte interestGroup, Hashtable customEventContent, bool sendReliable)
        {
            Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
            opParameters[(byte)LiteOpKey.Data] = customEventContent;
            opParameters[(byte)LiteOpKey.Code] = (byte)eventCode;
            if (interestGroup != (byte)0)
            {
                opParameters[(byte)LiteOpKey.Group] = (byte)interestGroup;
            }

            return this.OpCustom((byte)LiteOpCode.RaiseEvent, opParameters, sendReliable, 0);
        }

        /// <summary>
        /// Used in a room to raise (send) an event to the other players.
        /// Multiple overloads expose different parameters to this frequently used operation.
        /// This is an async request will trigger a OnOperationResponse() call only in error-cases,
        /// because it's called many times per second and can hardly fail.
        /// </summary>
        /// <param name="eventCode">Code for this "type" of event (assign a code, "meaning" and content at will, starting at code 1).</param>
        /// <param name="evData">Data to send. Hashtable that contains key-values of Photon serializable datatypes.</param>
        /// <param name="sendReliable">Use false if the event is replaced by a newer rapidly. Reliable events add overhead and add lag when repeated.</param>
        /// <param name="channelId">The "channel" to which this event should belong. Per channel, the sequence is kept in order.</param>
        /// <returns>If the operation could be sent (has to be connected).</returns>
        public virtual bool OpRaiseEvent(byte eventCode, Hashtable evData, bool sendReliable, byte channelId)
        {
            return this.OpRaiseEvent(eventCode, evData, sendReliable, channelId, EventCaching.DoNotCache, ReceiverGroup.Others);
        }

        /// <summary>
        /// Used in a room to raise (send) an event to the other players.
        /// Multiple overloads expose different parameters to this frequently used operation.
        /// </summary>
        /// <param name="eventCode">Code for this "type" of event (use a code per "meaning" or content).</param>
        /// <param name="evData">Data to send. Hashtable that contains key-values of Photon serializable datatypes.</param>
        /// <param name="sendReliable">Use false if the event is replaced by a newer rapidly. Reliable events add overhead and add lag when repeated.</param>
        /// <param name="channelId">The "channel" to which this event should belong. Per channel, the sequence is kept in order.</param>
        /// <param name="targetActors">Defines the target players who should receive the event (use only for small target groups).</param>
        /// <returns>If the operation could be sent (has to be connected).</returns>
        public virtual bool OpRaiseEvent(byte eventCode, Hashtable evData, bool sendReliable, byte channelId, int[] targetActors)
        {
            return this.OpRaiseEvent(eventCode, evData, sendReliable, channelId, targetActors, EventCaching.DoNotCache);
        }

        /// <summary>
        /// Used in a room to raise (send) an event to the other players.
        /// Multiple overloads expose different parameters to this frequently used operation.
        /// </summary>
        /// <param name="eventCode">Code for this "type" of event (use a code per "meaning" or content).</param>
        /// <param name="evData">Data to send. Hashtable that contains key-values of Photon serializable datatypes.</param>
        /// <param name="sendReliable">Use false if the event is replaced by a newer rapidly. Reliable events add overhead and add lag when repeated.</param>
        /// <param name="channelId">The "channel" to which this event should belong. Per channel, the sequence is kept in order.</param>
        /// <param name="targetActors">Defines the target players who should receive the event (use only for small target groups).</param>
        /// <param name="cache">Use EventCaching options to store this event for players who join.</param>
        /// <returns>If the operation could be sent (has to be connected).</returns>
        public virtual bool OpRaiseEvent(byte eventCode, Hashtable evData, bool sendReliable, byte channelId, int[] targetActors, EventCaching cache)
        {
            if (this.DebugOut >= DebugLevel.INFO)
            {
                this.Listener.DebugReturn(DebugLevel.INFO, "OpRaiseEvent()");
            }

            Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
            opParameters[ParameterCode.Data] = evData;
            opParameters[ParameterCode.Code] = (byte)eventCode;

            if (cache != EventCaching.DoNotCache)
            {
                opParameters[ParameterCode.Cache] = (byte)cache;
            }

            if (targetActors != null)
            {
                opParameters[ParameterCode.ActorList] = targetActors;
            }

            return this.OpCustom(OperationCode.RaiseEvent, opParameters, sendReliable, channelId);
        }

        /// <summary>
        /// Used in a room to raise (send) an event to the other players.
        /// Multiple overloads expose different parameters to this frequently used operation.
        /// </summary>
        /// <param name="eventCode">Code for this "type" of event (use a code per "meaning" or content).</param>
        /// <param name="evData">Data to send. Hashtable that contains key-values of Photon serializable datatypes.</param>
        /// <param name="sendReliable">Use false if the event is replaced by a newer rapidly. Reliable events add overhead and add lag when repeated.</param>
        /// <param name="channelId">The "channel" to which this event should belong. Per channel, the sequence is kept in order.</param>
        /// <param name="cache">Use EventCaching options to store this event for players who join.</param>
        /// <param name="receivers">ReceiverGroup defines to which group of players the event is passed on.</param>
        /// <returns>If the operation could be sent (has to be connected).</returns>
        public virtual bool OpRaiseEvent(byte eventCode, Hashtable evData, bool sendReliable, byte channelId, EventCaching cache, ReceiverGroup receivers)
        {
            if (this.DebugOut >= DebugLevel.INFO)
            {
                this.Listener.DebugReturn(DebugLevel.INFO, "OpRaiseEvent()");
            }

            Dictionary<byte, object> opParameters = new Dictionary<byte, object>();
            opParameters[ParameterCode.Data] = evData;
            opParameters[ParameterCode.Code] = (byte)eventCode;

            if (receivers != ReceiverGroup.Others)
            {
                opParameters[ParameterCode.ReceiverGroup] = (byte)receivers;
            }

            if (cache != EventCaching.DoNotCache)
            {
                opParameters[ParameterCode.Cache] = (byte)cache;
            }

            return this.OpCustom((byte)OperationCode.RaiseEvent, opParameters, sendReliable, channelId);
        }
    }

    /// <summary>
    /// ErrorCode defines the default codes associated with Photon client/server communication.
    /// </summary>
    public class ErrorCode
    {
        /// <summary>(0) is always "OK", anything else an error or specific situation.</summary>
        public const int Ok = 0;

        // server - Photon low(er) level: <= 0

        /// <summary>
        /// (-3) Operation can't be executed yet (e.g. OpJoin can't be called before being authenticated, RaiseEvent cant be used before getting into a room).
        /// </summary>
        /// <remarks>
        /// Before you call any operations on the Cloud servers, the automated client workflow must complete its authorization.
        /// In PUN, wait until State is: JoinedLobby (with AutoJoinLobby = true) or ConnectedToMaster (AutoJoinLobby = false)
        /// </remarks>
        public const int OperationNotAllowedInCurrentState = -3;

        /// <summary>(-2) The operation you called is not implemented on the server (application) you connect to. Make sure you run the fitting applications.</summary>
        public const int InvalidOperationCode = -2;

        /// <summary>(-1) Something went wrong in the server. Try to reproduce and contact Exit Games.</summary>
        public const int InternalServerError = -1;

        // server - PhotonNetwork: 0x7FFF and down
        // logic-level error codes start with short.max

        /// <summary>(32767) Authentication failed. Possible cause: AppId is unknown to Photon (in cloud service).</summary>
        public const int InvalidAuthentication = 0x7FFF;

        /// <summary>(32766) GameId (name) already in use (can't create another). Change name.</summary>
        public const int GameIdAlreadyExists = 0x7FFF - 1;

        /// <summary>(32765) Game is full. This rarely happens when some player joined the room before your join completed.</summary>
        public const int GameFull = 0x7FFF - 2;

        /// <summary>(32764) Game is closed and can't be joined. Join another game.</summary>
        public const int GameClosed = 0x7FFF - 3;

        [Obsolete("No longer used, cause random matchmaking is no longer a process.")]
        public const int AlreadyMatched = 0x7FFF - 4;

        /// <summary>(32762) Not in use currently.</summary>
        public const int ServerFull = 0x7FFF - 5;

        /// <summary>(32761) Not in use currently.</summary>
        public const int UserBlocked = 0x7FFF - 6;

        /// <summary>(32760) Random matchmaking only succeeds if a room exists thats neither closed nor full. Repeat in a few seconds or create a new room.</summary>
        public const int NoRandomMatchFound = 0x7FFF - 7;

        /// <summary>(32758) Join can fail if the room (name) is not existing (anymore). This can happen when players leave while you join.</summary>
        public const int GameDoesNotExist = 0x7FFF - 9;

        /// <summary>(32757) Authorization on the Photon Cloud failed becaus the concurrent users (CCU) limit of the app's subscription is reached.</summary>
        /// <remarks>
        /// Unless you have a plan with "CCU Burst", clients might fail the authentication step during connect.
        /// Affected client are unable to call operations. Please note that players who end a game and return
        /// to the master server will disconnect and re-connect, which means that they just played and are rejected
        /// in the next minute / re-connect.
        /// This is a temporary measure. Once the CCU is below the limit, players will be able to connect an play again.
        ///
        /// OpAuthorize is part of connection workflow but only on the Photon Cloud, this error can happen.
        /// Self-hosted Photon servers with a CCU limited license won't let a client connect at all.
        /// </remarks>
        public const int MaxCcuReached = 0x7FFF - 10;

        /// <summary>(32756) Authorization on the Photon Cloud failed because the app's subscription does not allow to use a particular region's server.</summary>
        /// <remarks>
        /// Some subscription plans for the Photon Cloud are region-bound. Servers of other regions can't be used then.
        /// Check your master server address and compare it with your Photon Cloud Dashboard's info.
        /// https://cloud.exitgames.com/dashboard
        ///
        /// OpAuthorize is part of connection workflow but only on the Photon Cloud, this error can happen.
        /// Self-hosted Photon servers with a CCU limited license won't let a client connect at all.
        /// </remarks>
        public const int InvalidRegion = 0x7FFF - 11;

        /// <summary>
        /// (32755) Custom Authentication of the user failed due to setup reasons (see Cloud Dashboard) or the provided user data (like username or token). Check error message for details.
        /// </summary>
        public const int CustomAuthenticationFailed = 0x7FFF - 12;
    }


    /// <summary>
    /// These (byte) values define "well known" properties for an Actor / Player.
    /// </summary>
    /// <remarks>
    /// "Custom properties" have to use a string-type as key. They can be assigned at will.
    /// </remarks>
    public class ActorProperties
    {
        /// <summary>(255) Name of a player/actor.</summary>
        public const byte PlayerName = 255; // was: 1
    }

    /// <summary>
    /// These (byte) values are for "well known" room/game properties used in Photon Loadbalancing.
    /// </summary>
    /// <remarks>
    /// "Custom properties" have to use a string-type as key. They can be assigned at will.
    /// </remarks>
    public class GameProperties
    {
        /// <summary>(255) Max number of players that "fit" into this room. 0 is for "unlimited".</summary>
        public const byte MaxPlayers = 255;

        /// <summary>(254) Makes this room listed or not in the lobby on master.</summary>
        public const byte IsVisible = 254;

        /// <summary>(253) Allows more players to join a room (or not).</summary>
        public const byte IsOpen = 253;

        /// <summary>(252) Current count od players in the room. Used only in the lobby on master.</summary>
        public const byte PlayerCount = 252;

        /// <summary>(251) True if the room is to be removed from room listing (used in update to room list in lobby on master)</summary>
        public const byte Removed = 251;

        /// <summary>(250) A list of the room properties to pass to the RoomInfo list in a lobby. This is used in CreateRoom, which defines this list once per room.</summary>
        public const byte PropsListedInLobby = 250;

        /// <summary>Equivalent of Operation Join parameter CleanupCacheOnLeave.</summary>
        public const byte CleanupCacheOnLeave = 249;
    }

    /// <summary>
    /// These values are for events defined by Photon Loadbalancing.
    /// </summary>
    /// <remarks>They start at 255 and go DOWN. Your own in-game events can start at 0.</remarks>
    public class EventCode
    {
        /// <summary>(230) Initial list of RoomInfos (in lobby on Master)</summary>
        public const byte GameList = 230;

        /// <summary>(229) Update of RoomInfos to be merged into "initial" list (in lobby on Master)</summary>
        public const byte GameListUpdate = 229;

        /// <summary>(228) Currently not used. State of queueing in case of server-full</summary>
        public const byte QueueState = 228;

        /// <summary>(227) Currently not used. Event for matchmaking</summary>
        public const byte Match = 227;

        /// <summary>(226) Event with stats about this application (players, rooms, etc)</summary>
        public const byte AppStats = 226;

        /// <summary>(210) Internally used in case of hosting by Azure</summary>
        [Obsolete("TCP routing was removed after becoming obsolete.")]
        public const byte AzureNodeInfo = 210;

        /// <summary>(255) Event Join: someone joined the game. The new actorNumber is provided as well as the properties of that actor (if set in OpJoin).</summary>
        public const byte Join = (byte)LiteEventCode.Join;

        /// <summary>(254) Event Leave: The player who left the game can be identified by the actorNumber.</summary>
        public const byte Leave = (byte)LiteEventCode.Leave;

        /// <summary>(253) When you call OpSetProperties with the broadcast option "on", this event is fired. It contains the properties being set.</summary>
        public const byte PropertiesChanged = (byte)LiteEventCode.PropertiesChanged;

        /// <summary>(253) When you call OpSetProperties with the broadcast option "on", this event is fired. It contains the properties being set.</summary>
        [Obsolete("Use PropertiesChanged now.")]
        public const byte SetProperties = (byte)LiteEventCode.PropertiesChanged;
    }

    /// <summary>Codes for parameters of Operations and Events.</summary>
    public class ParameterCode
    {
        /// <summary>(230) Address of a (game) server to use.</summary>
        public const byte Address = 230;

        /// <summary>(229) Count of players in this application in a rooms (used in stats event)</summary>
        public const byte PeerCount = 229;

        /// <summary>(228) Count of games in this application (used in stats event)</summary>
        public const byte GameCount = 228;

        /// <summary>(227) Count of players on the master server (in this app, looking for rooms)</summary>
        public const byte MasterPeerCount = 227;

        /// <summary>(225) User's ID</summary>
        public const byte UserId = 225;

        /// <summary>(224) Your application's ID: a name on your own Photon or a GUID on the Photon Cloud</summary>
        public const byte ApplicationId = 224;

        /// <summary>(223) Not used currently (as "Position"). If you get queued before connect, this is your position</summary>
        public const byte Position = 223;

        /// <summary>(223) Modifies the matchmaking algorithm used for OpJoinRandom. Allowed parameter values are defined in enum MatchmakingMode.</summary>
        public const byte MatchMakingType = 223;

        /// <summary>(222) List of RoomInfos about open / listed rooms</summary>
        public const byte GameList = 222;

        /// <summary>(221) Internally used to establish encryption</summary>
        public const byte Secret = 221;

        /// <summary>(220) Version of your application</summary>
        public const byte AppVersion = 220;

        /// <summary>(210) Internally used in case of hosting by Azure</summary>
        [Obsolete("TCP routing was removed after becoming obsolete.")]
        public const byte AzureNodeInfo = 210;	// only used within events, so use: EventCode.AzureNodeInfo

        /// <summary>(209) Internally used in case of hosting by Azure</summary>
        [Obsolete("TCP routing was removed after becoming obsolete.")]
        public const byte AzureLocalNodeId = 209;

        /// <summary>(208) Internally used in case of hosting by Azure</summary>
        [Obsolete("TCP routing was removed after becoming obsolete.")]
        public const byte AzureMasterNodeId = 208;

        /// <summary>(255) Code for the gameId/roomName (a unique name per room). Used in OpJoin and similar.</summary>
        public const byte RoomName = (byte)LiteOpKey.GameId;

        /// <summary>(250) Code for broadcast parameter of OpSetProperties method.</summary>
        public const byte Broadcast = (byte)LiteOpKey.Broadcast;

        /// <summary>(252) Code for list of players in a room. Currently not used.</summary>
        public const byte ActorList = (byte)LiteOpKey.ActorList;

        /// <summary>(254) Code of the Actor of an operation. Used for property get and set.</summary>
        public const byte ActorNr = (byte)LiteOpKey.ActorNr;

        /// <summary>(249) Code for property set (Hashtable).</summary>
        public const byte PlayerProperties = (byte)LiteOpKey.ActorProperties;

        /// <summary>(245) Code of data/custom content of an event. Used in OpRaiseEvent.</summary>
        public const byte CustomEventContent = (byte)LiteOpKey.Data;

        /// <summary>(245) Code of data of an event. Used in OpRaiseEvent.</summary>
        public const byte Data = (byte)LiteOpKey.Data;

        /// <summary>(244) Code used when sending some code-related parameter, like OpRaiseEvent's event-code.</summary>
        /// <remarks>This is not the same as the Operation's code, which is no longer sent as part of the parameter Dictionary in Photon 3.</remarks>
        public const byte Code = (byte)LiteOpKey.Code;

        /// <summary>(248) Code for property set (Hashtable).</summary>
        public const byte GameProperties = (byte)LiteOpKey.GameProperties;

        /// <summary>
        /// (251) Code for property-set (Hashtable). This key is used when sending only one set of properties.
        /// If either ActorProperties or GameProperties are used (or both), check those keys.
        /// </summary>
        public const byte Properties = (byte)LiteOpKey.Properties;

        /// <summary>(253) Code of the target Actor of an operation. Used for property set. Is 0 for game</summary>
        public const byte TargetActorNr = (byte)LiteOpKey.TargetActorNr;

        /// <summary>(246) Code to select the receivers of events (used in Lite, Operation RaiseEvent).</summary>
        public const byte ReceiverGroup = (byte)LiteOpKey.ReceiverGroup;

        /// <summary>(247) Code for caching events while raising them.</summary>
        public const byte Cache = (byte)LiteOpKey.Cache;

        /// <summary>(241) Bool parameter of CreateGame Operation. If true, server cleans up roomcache of leaving players (their cached events get removed).</summary>
        public const byte CleanupCacheOnLeave = (byte)241;

        /// <summary>(240) Code for "group" operation-parameter (as used in Op RaiseEvent).</summary>
        public const byte Group = LiteOpKey.Group;

        /// <summary>(239) The "Remove" operation-parameter can be used to remove something from a list. E.g. remove groups from player's interest groups.</summary>
        public const byte Remove = LiteOpKey.Remove;

        /// <summary>(238) The "Add" operation-parameter can be used to add something to some list or set. E.g. add groups to player's interest groups.</summary>
        public const byte Add = LiteOpKey.Add;

        /// <summary>(217) This key's (byte) value defines the target custom authentication type/service the client connects with. Used in OpAuthenticate</summary>
        public const byte ClientAuthenticationType = 217;

        /// <summary>(216) This key's (string) value provides parameters sent to the custom authentication type/service the client connects with. Used in OpAuthenticate</summary>
        public const byte ClientAuthenticationParams = 216;

        /// <summary>(214) This key's (string or byte[]) value provides parameters sent to the custom authentication service setup in Photon Dashboard. Used in OpAuthenticate</summary>
        public const byte ClientAuthenticationData = 214;

        /// <summary>(215) Makes the server create a room if it doesn't exist. OpJoin uses this to always enter a room, unless it exists and is full/closed.</summary>
        public const byte CreateIfNotExists = 215;

        /// <summary>(1) Used in Op FindFriends request. Value must be string[] of friends to look up.</summary>
        public const byte FindFriendsRequestList = (byte)1;

        /// <summary>(1) Used in Op FindFriends response. Contains bool[] list of online states (false if not online).</summary>
        public const byte FindFriendsResponseOnlineList = (byte)1;

        /// <summary>(2) Used in Op FindFriends response. Contains string[] of room names ("" where not known or no room joined).</summary>
        public const byte FindFriendsResponseRoomIdList = (byte)2;

        /// <summary>(213) Used in matchmaking-related methods and when creating a room to name a lobby (to join or to attach a room to).</summary>
        public const byte LobbyName = (byte)213;

        /// <summary>(213) Used in matchmaking-related methods and when creating a room to define the type of a lobby. Combined with the lobby name this identifies the lobby.</summary>
        public const byte LobbyType = (byte)212;
    }

    /// <summary>
    /// Codes for parameters and events used in PhotonLoadbalancingAPI
    /// </summary>
    public class OperationCode
    {
        /// <summary>(230) Authenticates this peer and connects to a virtual application</summary>
        public const byte Authenticate = 230;

        /// <summary>(229) Joins lobby (on master)</summary>
        public const byte JoinLobby = 229;

        /// <summary>(228) Leaves lobby (on master)</summary>
        public const byte LeaveLobby = 228;

        /// <summary>(227) Creates a game (or fails if name exists)</summary>
        public const byte CreateGame = 227;

        /// <summary>(226) Join game (by name)</summary>
        public const byte JoinGame = 226;

        /// <summary>(225) Joins random game (on master)</summary>
        public const byte JoinRandomGame = 225;

        // public const byte CancelJoinRandom = 224; // obsolete, cause JoinRandom no longer is a "process". now provides result immediately

        /// <summary>(254) Code for OpLeave, to get out of a room.</summary>
        public const byte Leave = (byte)LiteOpCode.Leave;

        /// <summary>(253) Raise event (in a room, for other actors/players)</summary>
        public const byte RaiseEvent = (byte)LiteOpCode.RaiseEvent;

        /// <summary>(252) Set Properties (of room or actor/player)</summary>
        public const byte SetProperties = (byte)LiteOpCode.SetProperties;

        /// <summary>(251) Get Properties</summary>
        public const byte GetProperties = (byte)LiteOpCode.GetProperties;

        /// <summary>(248) Operation code to change interest groups in Rooms (Lite application and extending ones).</summary>
        public const byte ChangeGroups = (byte)LiteOpCode.ChangeGroups;

        /// <summary>(222) Request the rooms and online status for a list of friends (by name, which should be unique).</summary>
        public const byte FindFriends = 222;
    }

    /// <summary>
    /// Options for matchmaking rules for OpJoinRandom.
    /// </summary>
    public enum MatchmakingMode : byte
    {
        /// <summary>Fills up rooms (oldest first) to get players together as fast as possible. Default.</summary>
        /// <remarks>Makes most sense with MaxPlayers > 0 and games that can only start with more players.</remarks>
        FillRoom = 0,
        /// <summary>Distributes players across available rooms sequentially but takes filter into account. Without filter, rooms get players evenly distributed.</summary>
        SerialMatching = 1,
        /// <summary>Joins a (fully) random room. Expected properties must match but aside from this, any available room might be selected.</summary>
        RandomMatching = 2
    }

    /// <summary>
    /// Options for optional "Custom Authentication" services used with Photon. Used by OpAuthenticate after connecting to Photon.
    /// </summary>
    public enum CustomAuthenticationType : byte
    {
        /// <summary>Use a custom authentification service. Currently the only implemented option.</summary>
        Custom = 0,
        
        /// <summary>Authenticates users by their Steam Account. Set auth values accordingly!</summary>
        Steam = 1,

        /// <summary>Authenticates users by their Facebook Account. Set auth values accordingly!</summary>
        Facebook = 2,

        /// <summary>Disables custom authentification. Same as not providing any AuthenticationValues for connect (more precisely for: OpAuthenticate).</summary>
        None = byte.MaxValue
    }

    /// <summary>
    /// Options of lobby types available. Lobby types might be implemented in certain Photon versions and won't be available on older servers.
    /// </summary>
    public enum LobbyType :byte
    {
        /// <summary>This lobby is used unless another is defined by game or JoinRandom. Room-lists will be sent and JoinRandomRoom can filter by matching properties.</summary>
        Default = 1,
        /// <summary>This lobby type lists rooms like Default but JoinRandom has a parameter for SQL-like "where" clauses for filtering. This allows bigger, less, or and and combinations.</summary>
        SqlLobby = 2
    }

    /// <summary>
    /// Container for "Custom Authentication" values in Photon (default: user and token). Set AuthParameters before connecting - all else is handled.
    /// </summary>
    /// <remarks>
    /// Custom Authentication lets you verify end-users by some kind of login or token. It sends those
    /// values to Photon which will verify them before granting access or disconnecting the client.
    ///
    /// The Photon Cloud Dashboard will let you enable this feature and set important server values for it.
    /// https://cloud.exitgames.com/dashboard
    /// </remarks>
    public class AuthenticationValues
    {
        /// <summary>The type of custom authentication provider that should be used. Currently only "Custom" or "None" (turns this off).</summary>
        public CustomAuthenticationType AuthType = CustomAuthenticationType.Custom;

        /// <summary>This string must contain any (http get) parameters expected by the used authentication service. By default, username and token.</summary>
        /// <remarks>Standard http get parameters are used here and passed on to the service that's defined in the server (Photon Cloud Dashboard).</remarks>
        public string AuthParameters;

        /// <summary>After initial authentication, Photon provides a secret for this client / user, which is subsequently used as (cached) validation.</summary>
        public string Secret;

        /// <summary>Data to be passed-on to the auth service via POST. Default: null (not sent). Either string or byte[] (see setters).</summary>
        public object AuthPostData { get; private set; }

        /// <summary>Sets the data to be passed-on to the auth service via POST.</summary>
        /// <param name="byteData">Binary token / auth-data to pass on. Empty string will set AuthPostData to null.</param>
        public virtual void SetAuthPostData(string stringData)
        {
            this.AuthPostData = (string.IsNullOrEmpty(stringData)) ? null : stringData;
        }

        /// <summary>Sets the data to be passed-on to the auth service via POST.</summary>
        /// <param name="byteData">Binary token / auth-data to pass on.</param>
        public virtual void SetAuthPostData(byte[] byteData)
        {
            this.AuthPostData = byteData;
        }

        /// <summary>Creates the default parameter-string from a user- and token-value, escaping both. Alternatively set AuthParameters directly.</summary>
        /// <remarks>The default parameter string is: "username={user}&token={token}"</remarks>
        /// <param name="user">Name or other end-user ID used in custom authentication service.</param>
        /// <param name="token">Token provided by authentication service to be used on initial "login" to Photon.</param>
        public virtual void SetAuthParameters(string user, string token)
        {
            this.AuthParameters = "username=" + System.Uri.EscapeDataString(user) + "&token=" + System.Uri.EscapeDataString(token);
        }
    }
}