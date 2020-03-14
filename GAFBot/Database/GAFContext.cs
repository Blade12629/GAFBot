using System;
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
        
        public virtual DbSet<BotBets> BotBets { get; set; }
        public virtual DbSet<BotConfig> BotConfig { get; set; }
        public virtual DbSet<BotLog> BotLog { get; set; }
        public virtual DbSet<BotMaintenance> BotMaintenance { get; set; }
        public virtual DbSet<BotUsers> BotUsers { get; set; }
        public virtual DbSet<BotVerifications> BotVerifications { get; set; }
        public virtual DbSet<BotAnalyzerResult> BotAnalyzerResult { get; set; }
        public virtual DbSet<BotAnalyzerRank> BotAnalyzerRank { get; set; }
        public virtual DbSet<BotAnalyzerBaninfo> BotAnalyzerBanInfo { get; set; }
        public virtual DbSet<BotAnalyzerScore> BotAnalyzerScore { get; set; }
        public virtual DbSet<BotAnalyzerTourneyMatches> BotAnalyzerTourneyMatch { get; set; }
        public virtual DbSet<BotBirthday> BotBirthday { get; set; }
        public virtual DbSet<BotLocalization> BotLocalization { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql(Environment.GetEnvironmentVariable("DBConnectionString", EnvironmentVariableTarget.Process));
                //optionsBuilder.UseMySql(Program.DBConnectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BotAnalyzerTourneyMatches>(entity =>
            {
                entity.ToTable("bot_analyzer_tourney_matches");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Season)
                    .HasColumnName("season")
                    .HasColumnType("text");

                entity.Property(e => e.MatchId)
                    .HasColumnName("match_id")
                    .HasColumnType("bigint(20)");
            });

            modelBuilder.Entity<BotAnalyzerScore>(entity =>
            {
                entity.ToTable("bot_analyzer_score");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");


                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.MatchId)
                    .HasColumnName("match_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.Accuracy)
                    .HasColumnName("accuracy")
                    .HasColumnType("float");

                entity.Property(e => e.Mods)
                    .HasColumnName("mods")
                    .HasColumnType("text");

                entity.Property(e => e.Score)
                    .HasColumnName("score")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.MaxCombo)
                    .HasColumnName("max_combo")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Perfect)
                    .HasColumnName("perfect")
                    .HasColumnType("int(11)");

                entity.Property(e => e.PP)
                    .HasColumnName("pp")
                    .HasColumnType("float");

                entity.Property(e => e.Rank)
                    .HasColumnName("rank")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("datetime");

                entity.Property(e => e.Slot)
                    .HasColumnName("slot")
                    .HasColumnType("int(11)");

                entity.Property(e => e.Team)
                    .HasColumnName("team")
                    .HasColumnType("text");

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

            modelBuilder.Entity<BotAnalyzerResult>(entity =>
            {
                entity.ToTable("bot_analyzer_result");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");
                
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

                entity.Property(e => e.HighestScoreBeatmapId)
                    .HasColumnName("highest_score_beatmap_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.HighestScoreOsuId)
                    .HasColumnName("highest_score_osu_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.HighestScoreId)
                    .HasColumnName("highest_score_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.HighestAccuracyBeatmapId)
                    .HasColumnName("highest_accuracy_beatmap_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.HighestAccuracyOsuId)
                    .HasColumnName("highest_accuracy_osu_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.HighestAccuracyScoreId)
                    .HasColumnName("highest_accuracy_score_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.MvpUserOsuId)
                    .HasColumnName("mvp_user_osu_id")
                    .HasColumnType("bigint(20)");
            });

            modelBuilder.Entity<BotAnalyzerRank>(entity =>
            {
                entity.ToTable("bot_analyzer_rank");
                
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.MatchId)
                    .HasColumnName("match_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.PlayerOsuId)
                    .HasColumnName("player_osu_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.Place)
                    .HasColumnName("place")
                    .HasColumnType("int(11)");

                entity.Property(e => e.PlaceAccuracy)
                    .HasColumnName("place_accuracy")
                    .HasColumnType("int(11)");

                entity.Property(e => e.PlaceScore)
                    .HasColumnName("place_score")
                    .HasColumnType("int(11)");

                entity.Property(e => e.MvpScore)
                    .HasColumnName("mvp_score")
                    .HasColumnType("float");
            });

            modelBuilder.Entity<BotAnalyzerBaninfo>(entity =>
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
            
            modelBuilder.Entity<BotBets>(entity =>
            {
                entity.ToTable("bot_bets");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

                entity.Property(e => e.DiscordUserId)
                    .HasColumnName("discord_user_id")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.Matchid)
                    .HasColumnName("matchid")
                    .HasColumnType("bigint(20)");

                entity.Property(e => e.Team)
                    .IsRequired()
                    .HasColumnName("team")
                    .HasColumnType("longtext");
            });

            modelBuilder.Entity<BotConfig>(entity =>
            {
                entity.ToTable("bot_config");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int(11)");

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

                entity.Property(e => e.CurrentSeason)
                    .HasColumnName("current_season")
                    .HasColumnType("text");

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

                entity.Property(e => e.Points)
                    .HasColumnName("points")
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
