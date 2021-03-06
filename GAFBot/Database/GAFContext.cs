﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using GAFBot.Database.Models;

namespace GAFBot.Database
{
    public partial class GAFContext : DbContext
    {
        public GAFContext()
        {
        }

        public GAFContext(DbContextOptions<GAFContext> options)
            : base(options)
        {
        }
        
        public virtual DbSet<BotConfig> BotConfig { get; set; }
        public virtual DbSet<BotLog> BotLog { get; set; }
        public virtual DbSet<BotMaintenance> BotMaintenance { get; set; }
        public virtual DbSet<BotUsers> BotUsers { get; set; }
        public virtual DbSet<BotVerifications> BotVerifications { get; set; }
        public virtual DbSet<BotSeasonBaninfo> BotSeasonBanInfo { get; set; }
        public virtual DbSet<BotBirthday> BotBirthday { get; set; }
        public virtual DbSet<BotLocalization> BotLocalization { get; set; }
        public virtual DbSet<BotCountryCode> BotCountryCode { get; set; }

#if GAF
        public virtual DbSet<TeamPlayerList> TeamPlayerList { get; set; }
        public virtual DbSet<Team> Team { get; set; }
        public virtual DbSet<Player> Player { get; set; }
#endif

        public virtual DbSet<BotPick> BotPick { get; set; }
        public virtual DbSet<BotApiKey> BotApiKey { get; set; }
        public virtual DbSet<BotApiRegisterCode> BotApiRegisterCode { get; set; }
        public virtual DbSet<Beatmap> Beatmap { get; set; }
        public virtual DbSet<BeatmapMod> BeatmapMod { get; set; }
        public virtual DbSet<BotSeasonPlayer> BotSeasonPlayer { get; set; }
        public virtual DbSet<BotSeasonResult> BotSeasonResult { get; set; }
        public virtual DbSet<BotSeasonScore> BotSeasonScore { get; set; }
        public virtual DbSet<BotSeasonBeatmapMod> BotSeasonBeatmapMod { get; set; }
        public virtual DbSet<BotSeasonBeatmap> BotSeasonBeatmap { get; set; }
        public virtual DbSet<BotSeasonPlayerCardCache> BotSeasonPlayerCardCache { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql(Environment.GetEnvironmentVariable("DBConnectionString", EnvironmentVariableTarget.Process), builder =>
                {
                    builder.EnableRetryOnFailure(10, TimeSpan.FromSeconds(2), null);
                }).EnableSensitiveDataLogging();

                base.OnConfiguring(optionsBuilder);
                //optionsBuilder.UseMySql(Program.DBConnectionString);
            }
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BotTimer>(entity =>
            {
                entity.ToTable("bot_timer");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("bigint(20)");
                entity.Property(e => e.Enabled)
                    .HasColumnName("enabled")
                    .HasColumnType("tinyint(1)");
                entity.Property(e => e.StartTime)
                    .HasColumnName("start_time")
                    .HasColumnType("datetime");
                entity.Property(e => e.EndTime)
                    .HasColumnName("end_time")
                    .HasColumnType("datetime");
                entity.Property(e => e.PingMessage)
                    .HasColumnName("ping_message")
                    .HasColumnType("text")
                    .HasDefaultValue("no info given");
                entity.Property(e => e.CreatedByDiscordId)
                    .HasColumnName("created_by_discord_id")
                    .HasColumnType("bigint(20)");
                entity.Property(e => e.DiscordChannelId)
                    .HasColumnName("discord_channel_id")
                    .HasColumnType("bigint(20)");
                entity.Property(e => e.IsPrivateChannel)
                    .HasColumnName("is_private_channel")
                    .HasColumnType("tinyint(1)");
                entity.Property(e => e.Expired)
                    .HasColumnName("expired")
                    .HasColumnType("tinyint(1)");
            });

            modelBuilder.Entity<BotSeasonPlayer>(entity =>
            {
                entity.ToTable("bot_season_player");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.OsuUserId)
                    .HasColumnName("osu_user_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.LastOsuUserName)
                    .HasColumnName("last_osu_user_name")
                    .HasColumnType("text");

                entity.Property(e => e.TeamName)
                    .HasColumnName("team_name")
                    .HasColumnType("text");
            });

            modelBuilder.Entity<BotSeasonResult>(entity =>
            {
                entity.ToTable("bot_season_result");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.Season)
                    .HasColumnName("season")
                    .HasColumnType("text");

                entity.Property(e => e.MatchId)
                    .HasColumnName("match_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.Stage)
                    .HasColumnName("stage")
                    .HasColumnType("text");

                entity.Property(e => e.MatchName)
                    .HasColumnName("match_name")
                    .HasColumnType("text");

                entity.Property(e => e.WinningTeam)
                    .HasColumnName("winning_team")
                    .HasColumnType("text");

                entity.Property(e => e.WinningTeamWins)
                    .HasColumnName("winning_team_wins")
                    .HasColumnType("int(11)");

                entity.Property(e => e.WinningTeamColor)
                    .HasColumnName("winning_team_color")
                    .HasColumnType("int(11)");

                entity.Property(e => e.LosingTeam)
                    .HasColumnName("losing_team")
                    .HasColumnType("text");

                entity.Property(e => e.LosingTeamWins)
                    .HasColumnName("losing_team_wins")
                    .HasColumnType("int(11)");

                entity.Property(e => e.TimeStamp)
                    .HasColumnName("time_stamp")
                    .HasColumnType("datetime");
            });

            modelBuilder.Entity<BotSeasonScore>(entity =>
            {
                entity.ToTable("bot_season_score");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("bigint(11)");

                entity.Property(e => e.BeatmapId)
                    .HasColumnName("beatmap_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.BotSeasonPlayerId)
                    .HasColumnName("bot_season_player_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.BotSeasonResultId)
                    .HasColumnName("bot_season_result_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.TeamName)
                    .HasColumnName("team_name")
                    .HasColumnType("text");

                entity.Property(e => e.TeamVs)
                    .HasColumnName("team_vs")
                    .HasColumnType("short(1)");

                entity.Property(e => e.PlayOrder)
                    .HasColumnName("play_order")
                    .HasColumnType("int(11)");

                entity.Property(e => e.GPS)
                    .HasColumnName("gps")
                    .HasColumnType("double");

                entity.Property(e => e.HighestGPS)
                    .HasColumnName("highest_gps")
                    .HasColumnType("short(1)");

                entity.Property(e => e.Accuracy)
                    .HasColumnName("accuracy")
                    .HasColumnType("float");

                entity.Property(e => e.Score)
                    .HasColumnName("score")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.MaxCombo)
                    .HasColumnName("max_combo")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Perfect)
                    .HasColumnName("perfect")
                    .HasColumnType("int(11)");

                entity.Property(e => e.PlayedAt)
                    .HasColumnName("played_at")
                    .HasColumnType("datetime");

                entity.Property(e => e.Pass)
                    .HasColumnName("pass")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Count50)
                    .HasColumnName("count_50")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Count100)
                    .HasColumnName("count_100")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Count300)
                    .HasColumnName("count_300")
                    .HasColumnType("int(11)");

                entity.Property(e => e.CountGeki)
                    .HasColumnName("count_geki")
                    .HasColumnType("int(11)");

                entity.Property(e => e.CountKatu)
                    .HasColumnName("count_katu")
                    .HasColumnType("int(11)");

                entity.Property(e => e.CountMiss)
                    .HasColumnName("count_miss")
                    .HasColumnType("int(11)");
            });

            modelBuilder.Entity<BotSeasonBeatmapMod>(entity =>
            {
                entity.ToTable("bot_season_beatmap_mod");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.BotSeasonScoreId)
                    .HasColumnName("bot_season_score_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.Mod)
                    .HasColumnName("mod")
                    .HasColumnType("text");
            });

            modelBuilder.Entity<BotSeasonBeatmap>(entity =>
            {
                entity.ToTable("bot_season_beatmap");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.BeatmapId)
                    .HasColumnName("beatmap_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.Author)
                    .HasColumnName("author")
                    .HasColumnType("text");

                entity.Property(e => e.Difficulty)
                    .HasColumnName("difficulty")
                    .HasColumnType("text");

                entity.Property(e => e.DifficultyRating)
                    .HasColumnName("difficulty_rating")
                    .HasColumnType("double");

                entity.Property(e => e.Title)
                    .HasColumnName("title")
                    .HasColumnType("text");
            });

            modelBuilder.Entity<BotSeasonPlayerCardCache>(entity =>
            {
                entity.ToTable("bot_season_player_card_cache");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.OsuUserId)
                    .HasColumnName("osu_user_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.Username)
                    .HasColumnName("username")
                    .HasColumnType("text");

                entity.Property(e => e.TeamName)
                    .HasColumnName("team_name")
                    .HasColumnType("text");

                entity.Property(e => e.AverageAccuracy)
                    .HasColumnName("average_accuracy")
                    .HasColumnType("double");

                entity.Property(e => e.AverageCombo)
                    .HasColumnName("average_combo")
                    .HasColumnType("double");

                entity.Property(e => e.AverageMisses)
                    .HasColumnName("average_misses")
                    .HasColumnType("double");

                entity.Property(e => e.AverageScore)
                    .HasColumnName("average_score")
                    .HasColumnType("double");

                entity.Property(e => e.AveragePerformance)
                    .HasColumnName("average_performance")
                    .HasColumnType("double");

                entity.Property(e => e.OverallRating)
                    .HasColumnName("overall_rating")
                    .HasColumnType("double");

                entity.Property(e => e.MatchMvps)
                    .HasColumnName("match_mvps")
                    .HasColumnType("int(11)");

                entity.Property(e => e.LastUpdated)
                    .HasColumnName("last_updated")
                    .HasColumnType("datetime");
            });

            modelBuilder.Entity<BeatmapMod>(entity =>
            {
                entity.ToTable("beatmap_mod");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Type)
                    .HasColumnName("season")
                    .HasColumnType("text");
            });

            modelBuilder.Entity<BotCountryCode>(entity =>
            {
                entity.ToTable("bot_country_code");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.CountryCode)
                    .HasColumnName("country_code")
                    .HasColumnType("text");

                entity.Property(e => e.Country)
                    .HasColumnName("country")
                    .HasColumnType("text");
            });

            modelBuilder.Entity<Beatmap>(entity =>
            {
                entity.ToTable("beatmap");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.Author)
                    .HasColumnName("author")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.BeatmapId)
                    .HasColumnName("beatmap_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.Difficulty)
                    .HasColumnName("difficulty")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.Image)
                    .HasColumnName("image")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.MapsetId)
                    .HasColumnName("mapset_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.Title)
                    .HasColumnName("title")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.ModId)
                    .HasColumnName("mod_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.MappoolId)
                    .HasColumnName("mappool_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.IsInMappool)
                    .HasColumnName("is_in_mappool")
                    .HasColumnType("bit(1)");

            });

            modelBuilder.Entity<BotApiKey>(entity =>
            {
                entity.ToTable("bot_api_key");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.DiscordId)
                    .HasColumnName("discord_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.Key)
                    .HasColumnName("key")
                    .HasColumnType("text");
            });

            modelBuilder.Entity<BotApiRegisterCode>(entity =>
            {
                entity.ToTable("bot_api_register_code");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Code)
                    .HasColumnName("code")
                    .HasColumnType("text");

                entity.Property(e => e.DiscordId)
                    .HasColumnName("picked_by")
                    .HasColumnType("text");
            });

            modelBuilder.Entity<BotPick>(entity =>
            {
                entity.ToTable("bot_pick");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.PickedBy)
                    .HasColumnName("picked_by")
                    .HasColumnType("text");

                entity.Property(e => e.Match)
                    .HasColumnName("match")
                    .HasColumnType("text");

                entity.Property(e => e.Team)
                    .HasColumnName("team")
                    .HasColumnType("text");

                entity.Property(e => e.Image)
                    .HasColumnName("image")
                    .HasColumnType("text");
            });

#if GAF
            modelBuilder.Entity<Player>(entity =>
            {
                entity.ToTable("player");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.Image)
                    .HasColumnName("image")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.Nickname)
                    .HasColumnName("nickname")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.OsuId)
                    .HasColumnName("osu_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.TeamId)
                    .HasColumnName("team_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.Rank)
                    .HasColumnName("rank")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.PPRaw)
                    .HasColumnName("pp_raw")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.PP)
                    .HasColumnName("pp")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.IsInTeam)
                    .HasColumnName("is_in_team")
                    .HasColumnType("bool");

                entity.Property(e => e.Country)
                    .HasColumnName("country")
                    .HasColumnType("varchar(255)");
            });

            modelBuilder.Entity<TeamPlayerList>(entity =>
            {
                entity.ToTable("team_player_list");

                entity.HasNoKey();

                entity.Property(e => e.TeamId)
                    .HasColumnName("team_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.PlayerListId)
                    .HasColumnName("player_list_id")
                    .HasColumnType("bigint(20)");
            });

            modelBuilder.Entity<Team>(entity =>
            {
                entity.ToTable("team");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.Image)
                    .HasColumnName("image")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.AveragePP)
                    .HasColumnName("averagepp")
                    .HasColumnType("int(11)");

                entity.Property(e => e.MedianPP)
                    .HasColumnName("medianpp")
                    .HasColumnType("bigint(20)");
            });
#endif

            modelBuilder.Entity<BotSeasonBaninfo>(entity =>
            {
                entity.ToTable("bot_analyzer_baninfo");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.MatchId)
                    .HasColumnName("match_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.Artist)
                    .IsRequired()
                    .HasColumnName("artist")
                    .HasColumnType("text");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasColumnName("title")
                    .HasColumnType("text");

                entity.Property(e => e.Version)
                    .IsRequired()
                    .HasColumnName("version")
                    .HasColumnType("text");

                entity.Property(e => e.BannedBy)
                    .IsRequired()
                    .HasColumnName("banned_by")
                    .HasColumnType("text");
            });

            modelBuilder.Entity<BotConfig>(entity =>
            {
                entity.ToTable("bot_config");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.CurrentSeason)
                    .HasColumnName("current_season")
                    .HasColumnType("text");

                entity.Property(e => e.AnalyzeChannel)
                    .HasColumnName("analyze_channel")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.AutoSaveTime)
                    .HasColumnName("auto_save_time")
                    .HasColumnType("time");

                entity.Property(e => e.DiscordClientSecretEncrypted)
                    .IsRequired()
                    .HasColumnName("discord_client_secret_encrypted")
                    .HasColumnType("longtext");

                entity.Property(e => e.DiscordGuildId)
                    .HasColumnName("discord_guild_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.OsuApiKeyEncrypted)
                    .IsRequired()
                    .HasColumnName("osu_api_key_encrypted")
                    .HasColumnType("longtext");

                entity.Property(e => e.OsuIrcHost)
                    .IsRequired()
                    .HasColumnName("osu_irc_host")
                    .HasColumnType("tinytext");

                entity.Property(e => e.OsuIrcPasswordEncrypted)
                    .IsRequired()
                    .HasColumnName("osu_irc_password_encrypted")
                    .HasColumnType("longtext");

                entity.Property(e => e.OsuIrcPort)
                    .HasColumnName("osu_irc_port")
                    .HasColumnType("int(11)");

                entity.Property(e => e.OsuIrcUser)
                    .IsRequired()
                    .HasColumnName("osu_irc_user")
                    .HasColumnType("tinytext");

                entity.Property(e => e.RefereeRoleId)
                    .HasColumnName("referee_role_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.SetVerifiedName)
                    .HasColumnName("set_verified_name")
                    .HasColumnType("tinyint(1)");

                entity.Property(e => e.SetVerifiedRole)
                    .HasColumnName("set_verified_role")
                    .HasColumnType("tinyint(1)");

                entity.Property(e => e.VerifiedRoleId)
                    .HasColumnName("verified_role_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.WarmupMatchCount)
                    .HasColumnName("warmup_match_count")
                    .HasColumnType("int(11)");

                entity.Property(e => e.WebsiteHost)
                    .IsRequired()
                    .HasColumnName("website_host")
                    .HasColumnType("tinytext");

                entity.Property(e => e.WebsitePassEncrypted)
                    .IsRequired()
                    .HasColumnName("website_pass_encrypted")
                    .HasColumnType("longtext");

                entity.Property(e => e.WebsiteUser)
                    .IsRequired()
                    .HasColumnName("website_user")
                    .HasColumnType("tinytext");

                entity.Property(e => e.WelcomeChannel)
                    .HasColumnName("welcome_channel")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.WelcomeMessage)
                    .IsRequired()
                    .HasColumnName("welcome_message")
                    .HasColumnType("longtext");
            });

            modelBuilder.Entity<BotLog>(entity =>
            {
                entity.ToTable("bot_log");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Date)
                    .HasColumnName("date")
                    .HasColumnType("datetime");

                entity.Property(e => e.Message)
                    .IsRequired()
                    .HasColumnName("message")
                    .HasColumnType("longtext");

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasColumnName("type")
                    .HasColumnType("tinytext");
            });

            modelBuilder.Entity<BotMaintenance>(entity =>
            {
                entity.ToTable("bot_maintenance");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Enabled)
                    .HasColumnName("enabled")
                    .HasColumnType("tinyint(1)");

                entity.Property(e => e.Notification)
                    .IsRequired()
                    .HasColumnName("notification")
                    .HasColumnType("longtext");
            });

            modelBuilder.Entity<BotUsers>(entity =>
            {
                entity.ToTable("bot_users");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.AccessLevel)
                    .HasColumnName("access_level")
                    .HasColumnType("smallint(6)");

                entity.Property(e => e.DiscordId)
                    .HasColumnName("discord_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.IsVerified)
                    .HasColumnName("is_verified")
                    .HasColumnType("tinyint(1)")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.OsuUsername)
                    .HasColumnName("osu_username")
                    .HasColumnType("varchar(255)");

                entity.Property(e => e.OsuUserId)
                    .HasColumnName("osu_user_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.Points)
                    .HasColumnName("points")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.PointsPickEm)
                    .HasColumnName("points_pick_em")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.RegisteredOn).HasColumnName("registered_on");
            });

            modelBuilder.Entity<BotVerifications>(entity =>
            {
                entity.ToTable("bot_verifications");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasColumnName("code")
                    .HasColumnType("tinytext");

                entity.Property(e => e.DiscordUserId)
                    .HasColumnName("discord_user_id")
                    .HasColumnType("bigint(20)");
            });

            modelBuilder.Entity<BotBirthday>(entity =>
            {
                entity.ToTable("bot_birthday");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.DiscordId)
                    .HasColumnName("discord_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.Day)
                    .HasColumnName("day")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Month)
                    .HasColumnName("month")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Year)
                    .HasColumnName("year")
                    .HasColumnType("int(11)");
            });

            modelBuilder.Entity<BotLocalization>(entity =>
            {
                entity.ToTable("bot_localization");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Code)
                    .HasColumnName("code")
                    .HasColumnType("text");

                entity.Property(e => e.String)
                    .HasColumnName("string")
                    .HasColumnType("text");
            });
        }
    }
}
