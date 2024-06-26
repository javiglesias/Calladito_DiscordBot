﻿using Discord;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Interop;

namespace Calladito_DiscordBot
{
    //  TODO    eliminar frases desde el discord.
    //  TODO    eliminar automagicamente el mensaje del bot al rato
    //  TODO    añadir, mostrar y eliminar solo funciona en un chat privado con el bot.
    class Program
    {
        private DiscordSocketClient m_bot;
        GLOBALS gl = new GLOBALS();
        private JArray m_insultos_list = new JArray();
        public static Task Main(string[] args) => new Program().MainAsync();
        private Timer timer = new System.Timers.Timer(300000);
        private static IMessageChannel last_channel_talked;
        private static SocketVoiceChannel last_voice_channel_talked;
        private SocketUser last_user_talked;

        public async Task MainAsync()
        {
            m_insultos_list = (JArray)gl.read_insultos_file()["insultos"];
            m_bot = new DiscordSocketClient();
            await m_bot.LoginAsync(Discord.TokenType.Bot, GLOBALS.m_bot_token);
            await m_bot.StartAsync();
            timer.AutoReset = true;
            timer.Elapsed += new ElapsedEventHandler(ElapsedTimerToInsult);
            timer.Start();
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
                    if(AddNewInsulto(_after.Content.Substring("calla_tontito_show ".Length)))
                    {
                        _after.DeleteAsync();
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
                    string msg = "Los ultimos insultos agregados:\n";
                    int count = 15;
                    int tempCount;
                    Int32.TryParse(_after.Content.Substring("calla_tontito_show".Length), out tempCount);
                    if (tempCount <= count)
                        count = tempCount;
                    for (int i = 1; i <= count; i++)
                    {
                        if ((msg.Length + (m_insultos_list[m_insultos_list.Count() - i]).ToString().Length) < 2000)
                            msg += m_insultos_list[m_insultos_list.Count() - i] + "\n";
                        else
                            break;
                    }
                    await SendMessageToChannel(msg, _after.Channel as IMessageChannel);
                } else
                {
                    await SendInsultoToChannel(_after.Author.Mention + " (" + _after.Author.Username + ")", _after.Channel as IMessageChannel);
                }
                last_channel_talked = _after.Channel as IMessageChannel;
                last_user_talked = _after.Author;
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
                    if (last_channel_talked == null)
                        SendInsultoToPeople(_user);
                    last_user_talked = _user;
                    // Id = 696839428143972395
                    //Random rng = new Random();
                    //last_voice_channel_talked = _state2.VoiceChannel;
                    //var calladito_sound = "resources\\Sounds\\agarrame.wav";
                    //System.Diagnostics.Process process = new System.Diagnostics.Process();
                    //var ps1 = new ProcessStartInfo
                    //{
                    //    CreateNoWindow = false,
                    //    FileName = @"cmd.exe",
                    //    Arguments = $"\"D:\\GDC24\\ffmpeg\\bin\\ffmpeg.exe\" -i {calladito_sound} -ac 2 -f s16le -ar 44100 pipe:1",
                    //    RedirectStandardOutput = true,
                    //    UseShellExecute = true
                    //};
                    //process.StartInfo = ps1;
                    //process.Start();
                    //var std_output = process.StandardOutput.BaseStream;
                    //var voice_conn = await _state2.VoiceChannel.ConnectAsync();
                    //var discord_output = voice_conn.CreatePCMStream(Discord.Audio.AudioApplication.Music, 96000);
                    //await std_output.CopyToAsync(discord_output);
                    //discord_output.Flush();
                }
            }
        }
        /// <summary>
        /// Whispers a user with a Insulto
        /// </summary>
        /// <param name="_user"></param>
        /// <param name="_channel"></param>
        /// <returns></returns>
        private async Task SendInsultoToPeople(SocketUser _user)
        {
            Random rng = new Random();
            var calladito_text = m_insultos_list[rng.Next(0, m_insultos_list.Count)].ToString();
            _user.SendMessageAsync(calladito_text);
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
        /// It sends a message to a User.
        /// </summary>
        /// <param name="_message"></param>
        /// <param name="_channel"></param>
        /// <param name="_isTTS"></param>
        /// <returns></returns>
        private async Task SendMessageToUser(string _message, IMessageChannel _channel, bool _isTTS = false)
        {
            await _channel.SendMessageAsync(_message, _isTTS);
        }
        /// <summary>
        /// It sends an message to a specified channel.
        /// </summary>
        /// <param name="_user"></param>
        /// <param name="_channel"></param>
        /// <returns></returns>
        private async Task SendMessageToChannel(string _message, IMessageChannel _channel, bool _isTTS = false)
        {
            var msg = await _channel.SendMessageAsync(_message, _isTTS);
            await Task.Delay(10000);
            await msg.DeleteAsync();
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
        /// <summary>
        /// It calls when the timer has elapsed.
        /// </summary>
        private void ElapsedTimerToInsult(object sender, ElapsedEventArgs e)
        {
            if(last_channel_talked != null)
                SendInsultoToChannel(last_user_talked.Mention, last_channel_talked);
        }
    }
}
