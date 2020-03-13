using DSharpPlus.Entities;
using System;

namespace GAFBot.Commands.Json
{
    public class EmbedJson
    {
        public string content { get; set; }
        public Embed embed { get; set; }

        public DiscordEmbed BuildEmbed()
        {
            try
            {
                Thumbnail thumbnail = embed.thumbnail;
                Image image = embed.image;

                Author author = embed.author;
                Footer footer = embed.footer;

                DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                {
                    Title = embed.title,
                    Description = embed.description,
                    Url = embed.url,
                    Color = new DiscordColor(embed.color),
                    Timestamp = embed.timestamp,
                    ThumbnailUrl = thumbnail?.url ?? null,
                    ImageUrl = image?.url ?? null,
                };

                if (footer != null)
                {
                    builder.Footer = new DiscordEmbedBuilder.EmbedFooter()
                    {
                        IconUrl = footer?.icon_url ?? null,
                        Text = footer?.text ?? null,
                    };
                }

                if (author != null)
                {
                    builder.Author = new DiscordEmbedBuilder.EmbedAuthor()
                    {
                        IconUrl = author?.icon_url ?? null,
                        Name = author?.name ?? null,
                        Url = author?.url ?? null
                    };
                }

                if (embed.fields != null)
                {
                    foreach (Field f in embed.fields)
                        builder.AddField(f.name, f.value, f?.inline ?? false);
                }

                return builder.Build();
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString(), LogLevel.ERROR);
                throw ex;
            }
        }
    }

    public class Embed
    {
        public string title { get; set; }
        public string description { get; set; }
        public string url { get; set; }
        public int color { get; set; }
        public DateTime timestamp { get; set; }
        public Footer footer { get; set; }
        public Thumbnail thumbnail { get; set; }
        public Image image { get; set; }
        public Author author { get; set; }
        public Field[] fields { get; set; }
    }

    public class Footer
    {
        public string icon_url { get; set; }
        public string text { get; set; }
    }

    public class Thumbnail
    {
        public string url { get; set; }
    }

    public class Image
    {
        public string url { get; set; }
    }

    public class Author
    {
        public string name { get; set; }
        public string url { get; set; }
        public string icon_url { get; set; }
    }

    public class Field
    {
        public string name { get; set; }
        public string value { get; set; }
        public bool inline { get; set; }
    }
}
