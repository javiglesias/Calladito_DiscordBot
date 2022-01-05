using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calladito_DiscordBot
{
    class Program
    {
        private DiscordSocketClient m_bot;
        public static Task Main(string[] args) => new Program().MainAsync();

        public async Task MainAsync()
        {
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
                await SendInsultoToChannel(_after.Author.Username, _after.Channel as IMessageChannel);
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
                    var calladito_sound = GLOBALS.m_insultos_list[rng.Next(0, 3)];
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
        /// It sends an Insulto a channel specified.
        /// </summary>
        /// <param name="_user"></param>
        /// <param name="_channel"></param>
        /// <returns></returns>
        private async Task SendInsultoToChannel(string _user, IMessageChannel _channel)
        {
            Random rng = new Random();
            var calladito_text = GLOBALS.m_insultos_list[rng.Next(0, GLOBALS.m_insultos_list.Count)].ToString();
            await _channel.SendMessageAsync(_user + ", " + calladito_text, true);
        }
    }
}
