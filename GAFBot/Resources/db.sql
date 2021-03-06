SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET AUTOCOMMIT = 0;
START TRANSACTION;
SET time_zone = "+00:00";

DROP TABLE IF EXISTS `bot_api_key`;
CREATE TABLE `bot_api_key` (`id` int(11) NOT NULL, `discord_id` bigint(20) NOT NULL, `key` text NOT NULL) ENGINE=InnoDB DEFAULT CHARSET=latin2;

DROP TABLE IF EXISTS `bot_api_register_code`;
CREATE TABLE `bot_api_register_code` (`id` int(11) NOT NULL, `code` text NOT NULL, `picked_by` text NOT NULL) ENGINE=InnoDB DEFAULT CHARSET=latin2;

DROP TABLE IF EXISTS `bot_birthday`;
CREATE TABLE `bot_birthday` (`id` int(11) NOT NULL, `discord_id` bigint(20) NOT NULL, `day` int(11) NOT NULL, `month` int(11) NOT NULL, `year` int(11) NOT NULL) ENGINE=InnoDB DEFAULT CHARSET=latin2;

DROP TABLE IF EXISTS `bot_config`;
CREATE TABLE `bot_config`(`id` int(11) NOT NULL,`current_season` text NOT NULL,`discord_client_secret_encrypted` longtext NOT NULL,`osu_api_key_encrypted` longtext NOT NULL,`osu_irc_host` tinytext NOT NULL,`osu_irc_port` int(11) NOT NULL,`osu_irc_user` tinytext NOT NULL,`osu_irc_password_encrypted` longtext NOT NULL,`website_user` tinytext NOT NULL,`website_pass_encrypted` longtext NOT NULL,`website_host` tinytext NOT NULL,`warmup_match_count` int(11) NOT NULL,`analyze_channel` bigint(20) NOT NULL,`auto_save_time` time NOT NULL,`discord_guild_id` bigint(20) NOT NULL,`verified_role_id` bigint(20) NOT NULL,`welcome_message` longtext NOT NULL,`welcome_channel` bigint(20) NOT NULL,`referee_role_id` bigint(20) NOT NULL,`set_verified_role` tinyint(1) NOT NULL,`set_verified_name` tinyint(1) NOT NULL) ENGINE=InnoDB DEFAULT CHARSET=latin2;

DROP TABLE IF EXISTS `bot_country_code`;
CREATE TABLE `bot_country_code` (`id` int(11) NOT NULL, `country_code` text NOT NULL, `country` text NOT NULL) ENGINE=InnoDB DEFAULT CHARSET=latin2;

DROP TABLE IF EXISTS `bot_localization`;
CREATE TABLE `bot_localization` (`id` int(11) NOT NULL, `code` text NOT NULL, `string` text NOT NULL) ENGINE=InnoDB DEFAULT CHARSET=latin2;

DROP TABLE IF EXISTS `bot_log`;
CREATE TABLE `bot_log` (`id` bigint(20) NOT NULL, `date` datetime NOT NULL, `type` tinytext NOT NULL, `message` longtext NOT NULL) ENGINE=InnoDB DEFAULT CHARSET=latin2;

DROP TABLE IF EXISTS `bot_maintenance`;
CREATE TABLE `bot_maintenance` (`id` int(11) NOT NULL, `enabled` tinyint(1) NOT NULL, `notification` longtext NOT NULL) ENGINE=InnoDB DEFAULT CHARSET=latin2;

DROP TABLE IF EXISTS `bot_season_baninfo`;
CREATE TABLE `bot_season_baninfo` (`id` int(11) NOT NULL, `match_id` bigint(20) NOT NULL, `artist` text NOT NULL, `title` text NOT NULL,  `banned_by` text NOT NULL) ENGINE=InnoDB DEFAULT CHARSET=latin2;

DROP TABLE IF EXISTS `bot_season_beatmap`;
CREATE TABLE `bot_season_beatmap` (`id` bigint(20) NOT NULL, `beatmap_id` bigint(20) NOT NULL, `author` text NOT NULL, `difficulty` text NOT NULL, `difficulty_rating` double NOT NULL, `title` text NOT NULL) ENGINE=InnoDB DEFAULT CHARSET=latin2;

DROP TABLE IF EXISTS `bot_users`;
CREATE TABLE `bot_users` (`id` bigint(20) NOT NULL, `access_level` smallint(6) DEFAULT NULL, `discord_id` bigint(20) DEFAULT NULL, `osu_username` varchar(255) DEFAULT NULL, `osu_user_id` bigint(20) NOT NULL, `points` bigint(20) DEFAULT NULL, `points_pick_em` bigint(20) NOT NULL, `registered_on` datetime(6) DEFAULT NULL, `is_verified` tinyint(1) NOT NULL DEFAULT '0') ENGINE=InnoDB DEFAULT CHARSET=latin2;

DROP TABLE IF EXISTS `bot_verifications`;
CREATE TABLE `bot_verifications` (`id` int(11) NOT NULL, `discord_user_id` bigint(20) NOT NULL, `code` tinytext NOT NULL) ENGINE=InnoDB DEFAULT CHARSET=latin2;

DROP TABLE IF EXISTS `bot_season_beatmap_mod`;
CREATE TABLE `bot_season_beatmap_mod` (`id` bigint(20) NOT NULL, `bot_season_score_id` bigint(20) NOT NULL, `mod` text NOT NULL) ENGINE=InnoDB DEFAULT CHARSET=latin2;

DROP TABLE IF EXISTS `bot_season_player`;
CREATE TABLE `bot_season_player` (`id` bigint(20) NOT NULL, `osu_user_id` bigint(20) NOT NULL, `last_osu_user_name` text NOT NULL, `team_name` text NOT NULL) ENGINE=InnoDB DEFAULT CHARSET=latin2;

DROP TABLE IF EXISTS `bot_season_player_card_cache`;
CREATE TABLE `bot_season_player_card_cache` (`id` bigint(20) NOT NULL, `osu_user_id` bigint(20) NOT NULL, `username` text NOT NULL, `team_name` text NOT NULL, `average_accuracy` double NOT NULL, `average_score` double NOT NULL, `average_misses` double NOT NULL, `average_combo` double NOT NULL, `average_performance` double NOT NULL, `overall_rating` double NOT NULL, `match_mvps` int(11) NOT NULL, `last_updated` datetime NOT NULL) ENGINE=InnoDB DEFAULT CHARSET=latin2;

DROP TABLE IF EXISTS `bot_season_result`;
CREATE TABLE `bot_season_result` (`id` bigint(20) NOT NULL, `season` text NOT NULL, `match_id` bigint(20) NOT NULL, `stage` text NOT NULL, `match_name` text NOT NULL, `winning_team` text NOT NULL, `winning_team_wins` int(11) NOT NULL, `winning_team_color` int(11) NOT NULL, `losing_team` text NOT NULL, `losing_team_wins` int(11) NOT NULL, `time_stamp` datetime NOT NULL) ENGINE=InnoDB DEFAULT CHARSET=latin2;

DROP TABLE IF EXISTS `bot_season_score`;
CREATE TABLE `bot_season_score` (`id` bigint(20) NOT NULL, `beatmap_id` bigint(20) NOT NULL, `bot_season_player_id` bigint(20) NOT NULL, `bot_season_result_id` bigint(20) NOT NULL, `team_name` text NOT NULL, `team_vs` tinyint(1) NOT NULL, `play_order` int(11) NOT NULL, `gps` double NOT NULL, `highest_gps` tinyint(1) NOT NULL, `accuracy` float NOT NULL, `score` bigint(20) NOT NULL, `max_combo` int(11) NOT NULL, `perfect` int(11) NOT NULL, `played_at` datetime NOT NULL, `pass` int(11) NOT NULL, `count_50` int(11) NOT NULL, `count_100` int(11) NOT NULL, `count_300` int(11) NOT NULL, `count_geki` int(11) NOT NULL, `count_katu` int(11) NOT NULL, `count_miss` int(11) NOT NULL) ENGINE=InnoDB DEFAULT CHARSET=latin2;

DROP TABLE IF EXISTS `bot_timer`;
CREATE TABLE `bot_timer` (`id` bigint(20) NOT NULL, `enabled` tinyint(1) NOT NULL, `start_time` datetime NOT NULL, `end_time` datetime NOT NULL, `ping_message` text NOT NULL, `created_by_discord_id` bigint(20) NOT NULL, `discord_channel_id` bigint(20) NOT NULL, `is_private_channel` tinyint(1) NOT NULL, `expired` tinyint(1) NOT NULL) ENGINE=InnoDB DEFAULT CHARSET=latin2;


INSERT INTO `bot_maintenance` (`id`, `enabled`, `notification`) VALUES (1, 0, 'Hello World!');

INSERT INTO `bot_localization` (`id`, `code`, `string`) VALUES (1, 'analyzerGeneratedPerformanceScore', 'GPS');
INSERT INTO `bot_localization` (`id`, `code`, `string`) VALUES (2, 'analyzerAverageAcc', 'Average Acc'); 
INSERT INTO `bot_localization` (`id`, `code`, `string`) VALUES (3, 'analyzerMVP', 'Most Valuable Player'); 
INSERT INTO `bot_localization` (`id`, `code`, `string`) VALUES (4, 'analyzerFirst', 'First Place'); 
INSERT INTO `bot_localization` (`id`, `code`, `string`) VALUES (5, 'analyzerWon', 'won!'); 
INSERT INTO `bot_localization` (`id`, `code`, `string`) VALUES (6, 'verifyIDEmpty', 'Verification id cannot be empty'); 
INSERT INTO `bot_localization` (`id`, `code`, `string`) VALUES (7, 'analyzerAcc', 'Accuracy'); 
INSERT INTO `bot_localization` (`id`, `code`, `string`) VALUES (8, 'verifyUserIDNotFound', 'Could not find your userId'); 
INSERT INTO `bot_localization` (`id`, `code`, `string`) VALUES (9, 'analyzerMatchPlayed', 'Match played at'); 
INSERT INTO `bot_localization` (`id`, `code`, `string`) VALUES (10, 'verifyVerifying', 'Verifying...'); 
INSERT INTO `bot_localization` (`id`, `code`, `string`) VALUES (11, 'verifyAccountLinked', 'You have successfully linked your discord account'); 
INSERT INTO `bot_localization` (`id`, `code`, `string`) VALUES (12, 'analyzerHighestScore', 'Highest Score'); 
INSERT INTO `bot_localization` (`id`, `code`, `string`) VALUES (13, 'analyzerTeam', 'Team'); 
INSERT INTO `bot_localization` (`id`, `code`, `string`) VALUES (14, 'analyzerTeamRed', 'Team Red'); 
INSERT INTO `bot_localization` (`id`, `code`, `string`) VALUES (15, 'analyzerOnMap', 'on the map'); 
INSERT INTO `bot_localization` (`id`, `code`, `string`) VALUES (16, 'analyzerHighestAcc', 'Highest Accuracy'); 
INSERT INTO `bot_localization` (`id`, `code`, `string`) VALUES (17, 'verifyUserNotFound', 'Could not find user'); 
INSERT INTO `bot_localization` (`id`, `code`, `string`) VALUES (18, 'analyzerWith', 'with'); 
INSERT INTO `bot_localization` (`id`, `code`, `string`) VALUES (19, 'analyzerThird', 'Third Place'); 
INSERT INTO `bot_localization` (`id`, `code`, `string`) VALUES (20, 'verifyIDNotFound', 'Could not find your verification id'); 
INSERT INTO `bot_localization` (`id`, `code`, `string`) VALUES (21, 'analyzerTeamBlue', 'Team Blue'); 
INSERT INTO `bot_localization` (`id`, `code`, `string`) VALUES (22, 'verifyAccountAlreadyLinked', 'Your user account has already been linked to an discord account\nIf this is an error or is incorrect, please contact Skyfly on discord (??????#0284 (6*?)))'); 
INSERT INTO `bot_localization` (`id`, `code`, `string`) VALUES (23, 'analyzerWithPoints', 'Points and'); 
INSERT INTO `bot_localization` (`id`, `code`, `string`) VALUES (24, 'analyzerSecond', 'Second Place'); 
INSERT INTO `bot_localization` (`id`, `code`, `string`) VALUES (25, 'verifyAccountLinked2', 'to your osu! account');
INSERT INTO `bot_localization` (`id`, `code`, `string`) VALUES (26, 'verifyAlreadyActive', 'Your verification is already active, code: ');

INSERT INTO `bot_localization` (`id`, `code`, `string`) VALUES (27, 'cmdUsageBirthday', 'Cannot parse');
INSERT INTO `bot_localization` (`id`, `code`, `string`) VALUES (28, 'cmdOutputBirthday', 'Cannot add to db, WIP');
INSERT INTO `bot_localization` (`id`, `code`, `string`) VALUES (29, 'cmdErrorBirthdayParse', 'Cannot add to db, WIP');
INSERT INTO `bot_localization` (`id`, `code`, `string`) VALUES (30, 'cmdBirthdaySet', 'Cannot add to db, WIP');
INSERT INTO `bot_localization` (`id`, `code`, `string`) VALUES (31, 'cmdDescriptionBirthday', 'Sets your current birthday');
INSERT INTO `bot_localization` (`id`, `code`, `string`) VALUES (32, 'cmdDescriptionCompile', 'Compiles and runs c# code at runtime');
INSERT INTO `bot_localization` (`id`, `code`, `string`) VALUES (33, 'cmdUsageCompile', '!compile c# code');

INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (1, 'AD', '\"Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (2, 'MZ', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (3, 'NA', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (4, 'NC', 'Pacific');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (5, 'NE', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (6, 'NF', 'Pacific');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (7, 'NG', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (8, 'NI', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (9, 'NL', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (10, 'NO', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (11, 'NP', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (12, 'NR', 'Pacific');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (13, 'NU', 'Pacific');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (14, 'NZ', 'Pacific');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (15, 'OM', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (16, 'PA', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (17, 'PE', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (18, 'PF', 'Pacific');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (19, 'PG', 'Pacific');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (20, 'PH', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (21, 'PK', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (22, 'PL', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (23, 'PM', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (24, 'PN', 'Pacific');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (25, 'PR', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (26, 'PS', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (27, 'PT', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (28, 'PW', 'Pacific');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (29, 'MY', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (30, 'MX', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (31, 'MW', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (32, 'MV', 'Indian');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (33, 'LB', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (34, 'LC', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (35, 'LI', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (36, 'LK', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (37, 'LR', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (38, 'LS', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (39, 'LT', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (40, 'LU', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (41, 'LV', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (42, 'LY', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (43, 'MA', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (44, 'MC', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (45, 'MD', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (46, 'PY', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (47, 'ME', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (48, 'MG', 'Indian');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (49, 'MH', 'Pacific');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (50, 'MK', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (51, 'ML', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (52, 'MM', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (53, 'MN', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (54, 'MO', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (55, 'MP', 'Pacific');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (56, 'MQ', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (57, 'MR', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (58, 'MS', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (59, 'MT', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (60, 'MU', 'Indian');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (61, 'MF', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (62, 'LA', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (63, 'QA', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (64, 'RO', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (65, 'TM', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (66, 'TN', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (67, 'TO', 'Pacific');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (68, 'TR', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (69, 'TT', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (70, 'TV', 'Pacific');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (71, 'TW', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (72, 'TZ', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (73, 'UA', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (74, 'UG', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (75, 'UM', 'Pacific');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (76, 'US', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (77, 'UY', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (78, 'UZ', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (79, 'VA', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (80, 'VC', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (81, 'VE', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (82, 'VG', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (83, 'VI', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (84, 'VN', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (85, 'VU', 'Pacific');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (86, 'WF', 'Pacific');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (87, 'WS', 'Pacific');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (88, 'YE', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (89, 'YT', 'Indian');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (90, 'YU', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (91, 'ZA', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (92, 'TL', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (93, 'TK', 'Pacific');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (94, 'TJ', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (95, 'TH', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (96, 'RS', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (97, 'RU', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (98, 'RW', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (99, 'SA', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (100, 'SB', 'Pacific');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (101, 'SC', 'Indian');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (102, 'SD', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (103, 'SE', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (104, 'SG', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (105, 'SH', 'Atlantic');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (106, 'SI', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (107, 'SJ', 'Arctic');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (108, 'SK', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (109, 'RE', 'Indian');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (110, 'SL', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (111, 'SN', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (112, 'SO', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (113, 'SR', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (114, 'SS', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (115, 'ST', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (116, 'SV', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (117, 'SX', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (118, 'SY', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (119, 'SZ', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (120, 'TC', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (121, 'TD', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (122, 'TF', 'Indian');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (123, 'TG', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (124, 'SM', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (125, 'ZM', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (126, 'KZ', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (127, 'KW', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (128, 'BT', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (129, 'BV', 'Antarctica');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (130, 'BW', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (131, 'BY', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (132, 'BZ', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (133, 'CA', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (134, 'CC', 'Indian');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (135, 'CD', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (136, 'CF', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (137, 'CG', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (138, 'CH', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (139, 'CI', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (140, 'CK', 'Pacific');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (141, 'CL', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (142, 'CM', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (143, 'CN', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (144, 'CO', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (145, 'CR', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (146, 'CU', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (147, 'CV', 'Atlantic');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (148, 'CW', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (149, 'CX', 'Indian');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (150, 'CY', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (151, 'CZ', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (152, 'DE', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (153, 'DJ', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (154, 'DK', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (155, 'BS', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (156, 'BR', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (157, 'BQ', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (158, 'BO', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (159, 'AE', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (160, 'AF', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (161, 'AG', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (162, 'AI', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (163, 'AL', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (164, 'AM', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (165, 'AN', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (166, 'AO', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (167, 'AQ', 'Antarctica');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (168, 'AR', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (169, 'AS', 'Pacific');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (170, 'AT', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (171, 'AU', 'Australia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (172, 'DM', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (173, 'AW', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (174, 'AZ', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (175, 'BA', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (176, 'BB', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (177, 'BD', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (178, 'BE', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (179, 'BF', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (180, 'BG', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (181, 'BH', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (182, 'BI', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (183, 'BJ', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (184, 'BL', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (185, 'BM', 'Atlantic');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (186, 'BN', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (187, 'AX', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (188, 'KY', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (189, 'DO', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (190, 'EC', 'Pacific');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (191, 'HK', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (192, 'HN', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (193, 'HR', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (194, 'HT', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (195, 'HU', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (196, 'ID', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (197, 'IE', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (198, 'IL', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (199, 'IM', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (200, 'IN', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (201, 'IO', 'Indian');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (202, 'IQ', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (203, 'IR', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (204, 'IS', 'Atlantic');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (205, 'IT', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (206, 'JE', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (207, 'JM', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (208, 'JO', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (209, 'JP', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (210, 'KE', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (211, 'KG', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (212, 'KH', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (213, 'KI', 'Pacific');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (214, 'KM', 'Indian');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (215, 'KN', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (216, 'KP', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (217, 'KR', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (218, 'GY', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (219, 'GW', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (220, 'GU', 'Pacific');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (221, 'GT', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (222, 'EE', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (223, 'EG', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (224, 'EH', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (225, 'ER', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (226, 'ES', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (227, 'ET', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (228, 'FI', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (229, 'FJ', 'Pacific');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (230, 'FK', 'Atlantic');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (231, 'FM', 'Pacific');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (232, 'FO', 'Atlantic');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (233, 'FR', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (234, 'FX', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (235, 'DZ', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (236, 'GA', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (237, 'GD', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (238, 'GE', 'Asia');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (239, 'GF', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (240, 'GG', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (241, 'GH', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (242, 'GI', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (243, 'GL', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (244, 'GM', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (245, 'GN', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (246, 'GP', 'America');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (247, 'GQ', 'Africa');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (248, 'GR', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (249, 'GS', 'Atlantic');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (250, 'GB', 'Europe');
INSERT INTO `bot_country_code` (`id`, `country_code`, `country`) VALUES (251, 'ZW', 'Africa');

ALTER TABLE `bot_api_key` ADD PRIMARY KEY (`id`);
ALTER TABLE `bot_api_register_code` ADD PRIMARY KEY (`id`);
ALTER TABLE `bot_maintenance` ADD PRIMARY KEY (`id`);
ALTER TABLE `bot_localization` ADD PRIMARY KEY (`id`);
ALTER TABLE `bot_season_baninfo` ADD PRIMARY KEY (`id`);
ALTER TABLE `bot_birthday` ADD PRIMARY KEY (`id`);
ALTER TABLE `bot_log` ADD PRIMARY KEY (`id`);
ALTER TABLE `bot_users` ADD PRIMARY KEY (`id`), ADD UNIQUE KEY `discord_id` (`discord_id`);
ALTER TABLE `bot_verifications` ADD PRIMARY KEY (`id`);
ALTER TABLE `bot_config` ADD PRIMARY KEY (`id`);
ALTER TABLE `bot_season_beatmap` ADD PRIMARY KEY (`id`);
ALTER TABLE `bot_season_beatmap_mod` ADD PRIMARY KEY (`id`);
ALTER TABLE `bot_season_player` ADD PRIMARY KEY (`id`);
ALTER TABLE `bot_season_player_card_cache` ADD PRIMARY KEY (`id`);
ALTER TABLE `bot_season_result` ADD PRIMARY KEY (`id`);
ALTER TABLE `bot_season_score` ADD PRIMARY KEY (`id`);
ALTER TABLE `bot_timer` ADD PRIMARY KEY (`id`);
ALTER TABLE `bot_country_code` ADD PRIMARY KEY (`id`);

ALTER TABLE `bot_api_key` MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
ALTER TABLE `bot_api_register_code` MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
ALTER TABLE `bot_maintenance` MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=2;
ALTER TABLE `bot_localization` MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=34;
ALTER TABLE `bot_season_baninfo` MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
ALTER TABLE `bot_birthday` MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
ALTER TABLE `bot_log` MODIFY `id` bigint(20) NOT NULL AUTO_INCREMENT;
ALTER TABLE `bot_users` MODIFY `id` bigint(20) NOT NULL AUTO_INCREMENT;
ALTER TABLE `bot_verifications` MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
ALTER TABLE `bot_config` MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
ALTER TABLE `bot_country_code` MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=252;
ALTER TABLE `bot_season_beatmap` MODIFY `id` bigint(20) NOT NULL AUTO_INCREMENT;
ALTER TABLE `bot_season_beatmap_mod` MODIFY `id` bigint(20) NOT NULL AUTO_INCREMENT;
ALTER TABLE `bot_season_player` MODIFY `id` bigint(20) NOT NULL AUTO_INCREMENT;
ALTER TABLE `bot_season_player_card_cache` MODIFY `id` bigint(20) NOT NULL AUTO_INCREMENT;
ALTER TABLE `bot_season_result` MODIFY `id` bigint(20) NOT NULL AUTO_INCREMENT;
ALTER TABLE `bot_season_score` MODIFY `id` bigint(20) NOT NULL AUTO_INCREMENT;
ALTER TABLE `bot_timer` MODIFY `id` bigint(20) NOT NULL AUTO_INCREMENT;