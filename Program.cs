using Discord;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calladito_DiscordBot
{
    //  TODO molaria poder añadir, eliminar y mostrar las frases desde el discord.
    class Program
    {
        private DiscordSocketClient m_bot;
        GLOBALS gl = new GLOBALS();
        private JArray m_insultos_list = new JArray();
        public static Task Main(string[] args) => new Program().MainAsync();

        public async Task MainAsync()
        {
            m_insultos_list = (JArray)gl.read_insultos_file()["insultos"];
            m_bot = new DiscordSocketClient();
            await m_bot.LoginAsync(Discord.TokenType.Bot, GLOBALS.m_bot_token);
            await m_bot.StartAsync();
            //  Subscribe to events
            m_bot.MessageReceived   += MessageReceived;
            m_bot.UserVoiceStateUpdated += UserVoiceStateUpdated;
            while (true)
            {
                //  Core Loop
                await Task.Delay(1000);
            }
        }
        /// <summary>
        /// When a messege is sent to a channel, triggers this event.
        /// </summary>
        /// <param name="_after"></param>
        /// <returns></returns>
        private async Task MessageReceived(SocketMessage _after)
        {
            if (!_after.Author.IsBot)
            {
                if(_after.Content.Contains("calla_tontito_add"))// Es un comando del Bot
                {
                    if(AddNewInsulto(_after.Content.Substring("calla_tontito_add".Length)))
                    {
                        await SendMessageToChannel("Insulto added.", _after.Channel as IMessageChannel);
                    } else
                    {
                        await SendMessageToChannel("Insulto duplicated", _after.Channel as IMessageChannel);
                    }
                }
                else if(_after.Content.Contains("calla_tontito_delete"))// Es un comando del Bot
                {
                    await SendMessageToChannel("Not Implemented.", _after.Channel as IMessageChannel);
                }
                else if(_after.Content.Contains("calla_tontito_show"))// Es un comando del Bot
                {
                    await SendMessageToChannel(m_insultos_list.ToString(), _after.Channel as IMessageChannel);
                } else
                {
                    await SendInsultoToChannel(_after.Author.Username, _after.Channel as IMessageChannel);
                }
            }
        }
        /// <summary>
        /// TODO: When someone connects to a voice channel, it should say something laik "Calladito, tontito"
        /// </summary>
        /// <param name="_user"></param>
        /// <param name="_state1"></param>
        /// <param name="_state2"></param>
        /// <returns></returns>
        private async Task UserVoiceStateUpdated(SocketUser _user, SocketVoiceState _state1, SocketVoiceState _state2)
        {
            //  Cuando se conecte alguien, que le suelte un "calladito, tontito" o algo asi.
            if(_state1.VoiceChannel == null && _state2.VoiceChannel != null)
            {
                if(!_user.IsBot)
                {
                    // Id = 696839428143972395
                    Random rng = new Random();
                    var voice_conn = await _state2.VoiceChannel.ConnectAsync();
                    var calladito_sound = m_insultos_list[rng.Next(0, 3)];
                    var ps1 = new ProcessStartInfo
                    {
                        FileName = @"ffmpeg.exe",
                        Arguments = $" -i {calladito_sound} -ac 2 -f s16le -ar 44100 pipe:1",
                        RedirectStandardOutput = true,
                        UseShellExecute = true
                    };
                    var ffmpeg = Process.Start(ps1);
                    var std_output = ffmpeg.StandardOutput.BaseStream;
                    var discord_output = voice_conn.CreatePCMStream(Discord.Audio.AudioApplication.Music, 96000);
                    await std_output.CopyToAsync(discord_output);
                    discord_output.Flush();
                }
            }
        }
        /// <summary>
        /// It sends an Insulto a specified channel.
        /// </summary>
        /// <param name="_user"></param>
        /// <param name="_channel"></param>
        /// <returns></returns>
        private async Task SendInsultoToChannel(string _user, IMessageChannel _channel)
        {
            Random rng = new Random();
            var calladito_text = m_insultos_list[rng.Next(0, m_insultos_list.Count)].ToString();
            await SendMessageToChannel(_user + ", " + calladito_text, _channel, true);
        }
        /// <summary>
        /// It sends an message to a specified channel.
        /// </summary>
        /// <param name="_user"></param>
        /// <param name="_channel"></param>
        /// <returns></returns>
        private async Task SendMessageToChannel(string _message, IMessageChannel _channel, bool _isTTS = false)
        {
            await _channel.SendMessageAsync(_message, _isTTS);
        }

        private bool AddNewInsulto(string insulto)
        {
            if(!m_insultos_list.Children().Contains(insulto))
            {
                m_insultos_list.Add(insulto);
                gl.add_insulto_to_file(m_insultos_list);
                return true;
            } else
            {
                return false;
            }
        }
    }
}
