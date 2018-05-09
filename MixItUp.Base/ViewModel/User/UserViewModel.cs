﻿using Mixer.Base.Model.Channel;
using Mixer.Base.Model.Chat;
using Mixer.Base.Model.Interactive;
using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.User
{
    public enum UserRole
    {
        Banned,
        User,
        Pro,
        Follower,
        Subscriber,
        Mod,
        Staff,
        Streamer,

        Custom = 99,
    }

    [DataContract]
    public class UserViewModel : IEquatable<UserViewModel>, IComparable<UserViewModel>
    {
        public const string DefaultAvatarLink = "https://mixer.com/_latest/assets/images/main/avatars/default.png";

        public static IEnumerable<UserRole> SelectableBasicUserRoles()
        {
            List<UserRole> roles = new List<UserRole>(EnumHelper.GetEnumList<UserRole>());
            roles.Remove(UserRole.Banned);
            roles.Remove(UserRole.Custom);
            return roles;
        }

        public static IEnumerable<string> SelectableAdvancedUserRoles()
        {
            List<string> roles = new List<string>(UserViewModel.SelectableBasicUserRoles().Select(r => EnumHelper.GetEnumName(r)));
            if (ChannelSession.Services.GameWisp != null)
            {
                foreach (GameWispTier tier in ChannelSession.Services.GameWisp.ChannelInfo.GetActiveTiers())
                {
                    roles.Add(tier.MIURoleName);
                }
            }
            return roles;
        }

        public uint ID { get; set; }

        public string UserName { get; set; }

        public string AvatarLink { get; set; }

        public uint GameTypeID { get; set; }

        public DateTimeOffset? MixerAccountDate { get; set; }

        public DateTimeOffset? FollowDate { get; set; }

        public DateTimeOffset? SubscribeDate { get; set; }

        public HashSet<UserRole> MixerRoles { get; set; }

        public HashSet<string> CustomRoles { get; set; }

        public int ChatOffenses { get; set; }

        public int Sparks { get; set; }

        public GameWispSubscriber GameWispUser { get; set; }

        public UserViewModel()
        {
            this.MixerRoles = new HashSet<UserRole>();
            this.CustomRoles = new HashSet<string>();
            this.AvatarLink = UserViewModel.DefaultAvatarLink;
        }

        public UserViewModel(UserModel user) : this(user.id, user.username)
        {
            this.AvatarLink = (!string.IsNullOrEmpty(user.avatarUrl)) ? user.avatarUrl : UserViewModel.DefaultAvatarLink;
            this.MixerAccountDate = user.createdAt;
        }

        public UserViewModel(ChannelModel channel) : this(channel.id, channel.token) { }

        public UserViewModel(ChatUserModel user) : this(user.userId.GetValueOrDefault(), user.userName, user.userRoles) { }

        public UserViewModel(ChatUserEventModel userEvent) : this(userEvent.id, userEvent.username, userEvent.roles) { }

        public UserViewModel(ChatMessageEventModel messageEvent) : this(messageEvent.user_id, messageEvent.user_name, messageEvent.user_roles) { }

        public UserViewModel(InteractiveParticipantModel participant) : this(participant.userID, participant.username) { }

        public UserViewModel(uint id, string username) : this(id, username, new string[] { }) { }

        public UserViewModel(UserDataViewModel user) : this(user.ID, user.UserName) { }

        public UserViewModel(uint id, string username, string[] userRoles)
            : this()
        {
            this.ID = id;
            this.UserName = username;

            this.SetUserRoles(userRoles);
        }

        public UserDataViewModel Data { get { return ChannelSession.Settings.UserData.GetValueIfExists(this.ID, new UserDataViewModel(this)); } }

        [JsonIgnore]
        public string RolesDisplayString
        {
            get
            {
                List<UserRole> mixerDisplayRoles = this.MixerRoles.ToList();
                if (this.MixerRoles.Contains(UserRole.Banned))
                {
                    mixerDisplayRoles.Clear();
                    mixerDisplayRoles.Add(UserRole.Banned);
                }
                else
                {
                    if (this.MixerRoles.Count() > 1)
                    {
                        mixerDisplayRoles.Remove(UserRole.User);
                    }

                    if (this.MixerRoles.Contains(UserRole.Subscriber) || this.MixerRoles.Contains(UserRole.Streamer))
                    {
                        mixerDisplayRoles.Remove(UserRole.Follower);
                    }

                    if (this.MixerRoles.Contains(UserRole.Streamer))
                    {
                        mixerDisplayRoles.Remove(UserRole.Subscriber);
                        mixerDisplayRoles.Remove(UserRole.Mod);
                    }
                }

                List<string> displayRoles = new List<string>(mixerDisplayRoles.Select(r => EnumHelper.GetEnumName(r)));
                displayRoles.AddRange(this.CustomRoles);

                return string.Join(", ", displayRoles.OrderByDescending(r => r));
            }
        }

        [JsonIgnore]
        public UserRole PrimaryRole { get { return this.MixerRoles.Max(); } }

        [JsonIgnore]
        public UserRole PrimarySortableRole { get { return this.MixerRoles.Where(r => r != UserRole.Follower).Max(); } }

        [JsonIgnore]
        public string MixerAgeString { get { return (this.MixerAccountDate != null) ? this.MixerAccountDate.GetValueOrDefault().GetAge() : "Unknown"; } }

        [JsonIgnore]
        public bool IsFollower { get { return this.MixerRoles.Contains(UserRole.Follower) || this.MixerRoles.Contains(UserRole.Streamer); } }

        [JsonIgnore]
        public string FollowAgeString { get { return (this.FollowDate != null) ? this.FollowDate.GetValueOrDefault().GetAge() : "Not Following"; } }

        [JsonIgnore]
        public bool IsSubscriber { get { return this.MixerRoles.Contains(UserRole.Subscriber) || this.MixerRoles.Contains(UserRole.Streamer); } }

        [JsonIgnore]
        public string SubscribeAgeString { get { return (this.SubscribeDate != null) ? this.SubscribeDate.GetValueOrDefault().GetAge() : "Not Subscribed"; } }

        [JsonIgnore]
        public int SubscribeMonths
        {
            get
            {
                int subMonths = 0;
                if (this.SubscribeDate != null)
                {
                    TimeSpan span = DateTimeOffset.Now.Date - this.SubscribeDate.GetValueOrDefault().Date;
                    double totalMonths = ((double)span.Days) / 30.0;

                    subMonths = (int)totalMonths;
                    totalMonths = totalMonths - subMonths;
                    if (totalMonths > 0.0)
                    {
                        subMonths++;
                    }
                }
                return subMonths;
            }
        }

        [JsonIgnore]
        public string PrimaryRoleColorName
        {
            get
            {
                if (this.MixerRoles.Contains(UserRole.Streamer))
                {
                    return "UserStreamerRoleColor";
                }
                else if (this.MixerRoles.Contains(UserRole.Staff))
                {
                    return "UserStaffRoleColor";
                }
                else if (this.MixerRoles.Contains(UserRole.Mod))
                {
                    return "UserModRoleColor";
                }
                else if (this.MixerRoles.Contains(UserRole.Pro))
                {
                    return "UserProRoleColor";
                }
                else
                {
                    return "UserDefaultRoleColor";
                }
            }
        }

        [JsonIgnore]
        public GameWispTier GameWispTier
        {
            get
            {
                if (ChannelSession.Services.GameWisp != null && this.GameWispUser != null)
                {
                    return ChannelSession.Services.GameWisp.ChannelInfo.GetActiveTiers().FirstOrDefault(t => t.ID.ToString().Equals(this.GameWispUser.TierID));
                }
                return null;
            }
        }

        public async Task SetDetails(bool checkForFollow = true)
        {
            if (this.ID > 0)
            {
                UserWithChannelModel userWithChannel = await ChannelSession.Connection.GetUser(this.GetModel());
                if (userWithChannel != null)
                {
                    if (!string.IsNullOrEmpty(userWithChannel.avatarUrl))
                    {
                        this.AvatarLink = userWithChannel.avatarUrl;
                    }
                    this.MixerAccountDate = userWithChannel.createdAt;
                    this.Sparks = (int)userWithChannel.sparks;
                    this.GameTypeID = userWithChannel.channel.typeId.GetValueOrDefault();

                    this.Data.UpdateData(userWithChannel);
                }

                await this.SetMixerRoles();
                await this.SetCustomRoles();
            }

            if (checkForFollow)
            {
                await this.SetFollowDate();

                if (this.MixerRoles.Contains(UserRole.Subscriber))
                {
                    await this.SetSubscribeDate();
                }
            }
        }

        public async Task SetMixerRoles()
        {
            if (this.ID > 0)
            {
                ChatUserModel chatUser = await ChannelSession.Connection.GetChatUser(ChannelSession.Channel, this.ID);
                if (chatUser != null)
                {
                    this.SetUserRoles(chatUser.userRoles);
                }
            }
        }

        public async Task SetCustomRoles()
        {
            if (this.ID > 0)
            {
                this.CustomRoles.Clear();
                if (ChannelSession.Services.GameWisp != null && this.Data.GameWispUserID > 0)
                {
                    if (this.GameWispUser == null)
                    {
                        await this.SetGameWispSubscriber();
                    }

                    if (this.GameWispTier != null)
                    {
                        this.CustomRoles.Add(this.GameWispTier.MIURoleName);
                    }
                }
            }
        }

        public async Task SetFollowDate()
        {
            if (this.FollowDate == null || this.FollowDate.GetValueOrDefault() == DateTimeOffset.MinValue)
            {
                this.FollowDate = await ChannelSession.Connection.CheckIfFollows(ChannelSession.Channel, this.GetModel());
                if (this.FollowDate != null && this.FollowDate.GetValueOrDefault() > DateTimeOffset.MinValue)
                {
                    this.MixerRoles.Add(UserRole.Follower);
                }
            }
        }

        public async Task SetSubscribeDate()
        {
            if (this.SubscribeDate == null || this.SubscribeDate.GetValueOrDefault() == DateTimeOffset.MinValue)
            {
                UserWithGroupsModel userGroups = await ChannelSession.Connection.GetUserInChannel(ChannelSession.Channel, this.ID);
                if (userGroups != null && userGroups.groups != null)
                {
                    UserGroupModel subscriberGroup = userGroups.groups.FirstOrDefault(g => g.name.Equals("Subscriber") && g.deletedAt == null);
                    if (subscriberGroup != null)
                    {
                        this.SubscribeDate = subscriberGroup.createdAt;
                        this.MixerRoles.Add(UserRole.Subscriber);
                    }
                }
            }
        }

        public async Task SetGameWispSubscriber()
        {
            if (ChannelSession.Services.GameWisp != null)
            {
                IEnumerable<GameWispSubscriber> subscribers = await ChannelSession.Services.GameWisp.GetCachedSubscribers();

                GameWispSubscriber subscriber = null;
                if (this.Data.GameWispUserID > 0)
                {
                    subscriber = await ChannelSession.Services.GameWisp.GetSubscriber(this.Data.GameWispUserID);
                }
                else
                {
                    subscriber = subscribers.FirstOrDefault(s => s.UserName.Equals(this.UserName, StringComparison.CurrentCultureIgnoreCase));
                }

                this.GameWispUser = subscriber;
                if (subscriber != null)
                {
                    this.Data.GameWispUserID = subscriber.UserID;
                }
            }
        }

        public UserModel GetModel()
        {
            return new UserModel()
            {
                id = this.ID,
                username = this.UserName,
            };
        }

        public ChatUserModel GetChatModel()
        {
            return new ChatUserModel()
            {
                userId = this.ID,
                userName = this.UserName,
                userRoles = this.MixerRoles.Select(r => r.ToString()).ToArray(),
            };
        }

        public override bool Equals(object obj)
        {
            if (obj is UserViewModel)
            {
                return this.Equals((UserViewModel)obj);
            }
            return false;
        }

        public bool Equals(UserViewModel other) { return this.ID.Equals(other.ID); }

        public int CompareTo(UserViewModel other)
        {
            int order = this.PrimaryRole.CompareTo(other.PrimaryRole);
            if (order == 0)
            {
                return this.UserName.CompareTo(other.UserName);
            }
            return order;
        }

        public override int GetHashCode() { return this.ID.GetHashCode(); }

        public override string ToString() { return this.UserName; }

        private void SetUserRoles(string[] userRoles)
        {
            this.MixerRoles.Clear();

            this.MixerRoles.Add(UserRole.User);
            if (userRoles != null)
            {
                if (userRoles.Any(r => r.Equals("Owner"))) { this.MixerRoles.Add(UserRole.Streamer); }
                if (userRoles.Any(r => r.Equals("Staff"))) { this.MixerRoles.Add(UserRole.Staff); }
                if (userRoles.Any(r => r.Equals("Mod"))) { this.MixerRoles.Add(UserRole.Mod); }
                if (userRoles.Any(r => r.Equals("Subscriber"))) { this.MixerRoles.Add(UserRole.Subscriber); }
                if (userRoles.Any(r => r.Equals("Pro"))) { this.MixerRoles.Add(UserRole.Pro); }
                if (userRoles.Any(r => r.Equals("Banned"))) { this.MixerRoles.Add(UserRole.Banned); }
            }

            if (this.MixerRoles.Contains(UserRole.Streamer))
            {
                this.MixerRoles.Add(UserRole.Subscriber);
                this.MixerRoles.Add(UserRole.Mod);
            }

            if (this.IsSubscriber)
            {
                this.MixerRoles.Add(UserRole.Follower);
            }
        }
    }
}
