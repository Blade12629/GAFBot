SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET AUTOCOMMIT = 0;
START TRANSACTION;
SET time_zone = "+00:00";

DROP TABLE IF EXISTS `bot_maintenance`;
CREATE TABLE `bot_maintenance` (`id` int(11) NOT NULL, `enabled` tinyint(1) NOT NULL, `notification` longtext NOT NULL) ENGINE=InnoDB DEFAULT CHARSET=latin2;

DROP TABLE IF EXISTS `bot_localization`;
CREATE TABLE `bot_localization` (`id` int(11) NOT NULL, `code` text NOT NULL, `string` text NOT NULL) ENGINE=InnoDB DEFAULT CHARSET=latin2;

DROP TABLE IF EXISTS `bot_analyzer_baninfo`;
CREATE TABLE `bot_analyzer_baninfo` (`id` int(11) NOT NULL, `match_id` bigint(20) NOT NULL, `artist` text NOT NULL, `title` text NOT NULL,  `banned_by` text NOT NULL) ENGINE=InnoDB DEFAULT CHARSET=latin2;

DROP TABLE IF EXISTS `bot_analyzer_rank`;
CREATE TABLE `bot_analyzer_rank` (`id` int(11) NOT NULL, `match_id` bigint(20) NOT NULL, `player_osu_id` bigint(20) NOT NULL, `place` int(11) NOT NULL, `place_accuracy` int(11) NOT NULL, `place_score` int(11) NOT NULL, `mvp_score` float NOT NULL) ENGINE=InnoDB DEFAULT CHARSET=latin2;

DROP TABLE IF EXISTS `bot_analyzer_result`;
CREATE TABLE `bot_analyzer_result` (`id` int(11) NOT NULL, `match_id` bigint(20) NOT NULL, `stage` text NOT NULL, `match_name` text NOT NULL, `winning_team` text NOT NULL, `winning_team_wins` int(11) NOT NULL, `winning_team_color` int(11) NOT NULL, `losing_team` text NOT NULL, `losing_team_wins` int(11) NOT NULL, `time_stamp` datetime NOT NULL, `highest_score_beatmap_id` bigint(20) NOT NULL, `highest_score_osu_id` bigint(20) NOT NULL, `highest_score_id` bigint(20) NOT NULL, `highest_accuracy_beatmap_id` bigint(20) NOT NULL, `highest_accuracy_osu_id` bigint(20) NOT NULL, `highest_accuracy_score_id` bigint(20) NOT NULL, `mvp_user_osu_id` bigint(20) NOT NULL) ENGINE=InnoDB DEFAULT CHARSET=latin2;

DROP TABLE IF EXISTS `bot_analyzer_score`;
CREATE TABLE `bot_analyzer_score` (`id` int(11) NOT NULL, `user_id` bigint(20) NOT NULL, `match_id` bigint(20) NOT NULL, `accuracy` float NOT NULL, `mods` text NOT NULL, `score` bigint(20) NOT NULL, `max_combo` int(11) NOT NULL, `perfect` int(11) NOT NULL, `pp` float NOT NULL, `rank` bigint(20) NOT NULL, `created_at` datetime NOT NULL, `slot` int(11) NOT NULL, `team` text NOT NULL, `pass` int(11) NOT NULL, `count_50` int(11) NOT NULL, `count_100` int(11) NOT NULL, `count_300` int(11) NOT NULL, `count_geki` int(11) NOT NULL, `count_katu` int(11) NOT NULL, `count_miss` int(11) NOT NULL) ENGINE=InnoDB DEFAULT CHARSET=latin2;

DROP TABLE IF EXISTS `bot_analyzer_tourney_matches`;
CREATE TABLE `bot_analyzer_tourney_matches` (`id` int(11) NOT NULL, `challonge_tournament_name` text NOT NULL, `match_id` bigint(20) NOT NULL) ENGINE=InnoDB DEFAULT CHARSET=latin2;

DROP TABLE IF EXISTS `bot_bets`;
CREATE TABLE `bot_bets` (`id` int(11) NOT NULL, `team` longtext NOT NULL, `matchid` bigint(20) NOT NULL, `discord_user_id` bigint(20) NOT NULL) ENGINE=InnoDB DEFAULT CHARSET=latin2;

DROP TABLE IF EXISTS `bot_birthday`;
CREATE TABLE `bot_birthday` (`id` int(11) NOT NULL, `discord_id` bigint(20) NOT NULL, `day` int(11) NOT NULL, `month` int(11) NOT NULL, `year` int(11) NOT NULL) ENGINE=InnoDB DEFAULT CHARSET=latin2;

DROP TABLE IF EXISTS `bot_log`;
CREATE TABLE `bot_log` (`id` bigint(20) NOT NULL, `date` datetime NOT NULL, `type` tinytext NOT NULL, `message` longtext NOT NULL) ENGINE=InnoDB DEFAULT CHARSET=latin2;

DROP TABLE IF EXISTS `bot_users`;
CREATE TABLE `bot_users` (`id` bigint(20) NOT NULL, `access_level` smallint(6) DEFAULT NULL, `discord_id` bigint(20) DEFAULT NULL, `osu_username` varchar(255) DEFAULT NULL, `points` bigint(20) DEFAULT NULL, `registered_on` datetime(6) DEFAULT NULL, `is_verified` tinyint(1) NOT NULL DEFAULT '0') ENGINE=InnoDB DEFAULT CHARSET=latin2;

DROP TABLE IF EXISTS `bot_verifications`;
CREATE TABLE `bot_verifications` (`id` int(11) NOT NULL, `discord_user_id` bigint(20) NOT NULL, `code` tinytext NOT NULL) ENGINE=InnoDB DEFAULT CHARSET=latin2;

DROP TABLE IF EXISTS `bot_config`;
CREATE TABLE `bot_config`(`id` int(11) NOT NULL,`current_season` text NOT NULL,`discord_client_secret_encrypted` longtext NOT NULL,`osu_api_key_encrypted` longtext NOT NULL,`osu_irc_host` tinytext NOT NULL,`osu_irc_port` int(11) NOT NULL,`osu_irc_user` tinytext NOT NULL,`osu_irc_password_encrypted` longtext NOT NULL,`website_user` tinytext NOT NULL,`website_pass_encrypted` longtext NOT NULL,`website_host` tinytext NOT NULL,`warmup_match_count` int(11) NOT NULL,`analyze_channel` bigint(20) NOT NULL,`auto_save_time` time NOT NULL,`discord_guild_id` bigint(20) NOT NULL,`verified_role_id` bigint(20) NOT NULL,`welcome_message` longtext NOT NULL,`welcome_channel` bigint(20) NOT NULL,`referee_role_id` bigint(20) NOT NULL,`set_verified_role` tinyint(1) NOT NULL,`set_verified_name` tinyint(1) NOT NULL) ENGINE=InnoDB DEFAULT CHARSET=latin2;

INSERT INTO `bot_maintenance` (`id`, `enabled`, `notification`) VALUES (1, 0, 'Hello World');

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

ALTER TABLE `bot_maintenance` ADD PRIMARY KEY (`id`);
ALTER TABLE `bot_localization` ADD PRIMARY KEY (`id`);
ALTER TABLE `bot_analyzer_baninfo` ADD PRIMARY KEY (`id`);
ALTER TABLE `bot_analyzer_rank` ADD PRIMARY KEY (`id`);
ALTER TABLE `bot_analyzer_result` ADD PRIMARY KEY (`id`);
ALTER TABLE `bot_analyzer_score` ADD PRIMARY KEY (`id`);
ALTER TABLE `bot_analyzer_tourney_matches` ADD PRIMARY KEY (`id`);
ALTER TABLE `bot_bets` ADD PRIMARY KEY (`id`);
ALTER TABLE `bot_birthday` ADD PRIMARY KEY (`id`);
ALTER TABLE `bot_log` ADD PRIMARY KEY (`id`);
ALTER TABLE `bot_users` ADD PRIMARY KEY (`id`), ADD UNIQUE KEY `discord_id` (`discord_id`);
ALTER TABLE `bot_verifications` ADD PRIMARY KEY (`id`);
ALTER TABLE `bot_config` ADD PRIMARY KEY (`id`);

ALTER TABLE `bot_maintenance` MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=2;
ALTER TABLE `bot_localization` MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=26;
ALTER TABLE `bot_analyzer_baninfo` MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
ALTER TABLE `bot_analyzer_rank` MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
ALTER TABLE `bot_analyzer_result` MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
ALTER TABLE `bot_analyzer_score` MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
ALTER TABLE `bot_analyzer_tourney_matches` MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
ALTER TABLE `bot_bets` MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
ALTER TABLE `bot_birthday` MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
ALTER TABLE `bot_log` MODIFY `id` bigint(20) NOT NULL AUTO_INCREMENT;
ALTER TABLE `bot_users` MODIFY `id` bigint(20) NOT NULL AUTO_INCREMENT;
ALTER TABLE `bot_verifications` MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;
ALTER TABLE `bot_config` MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=7;