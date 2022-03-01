using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace SampleBufferFetchInt32
{
    public class Startup
    {

        //Buffer �ϥ�
        private static System.Collections.Concurrent.ConcurrentQueue<Int32> _IndexPointerSwap;

        private static int _IndexPointer;

        /// <summary>
        /// �ˬd�ثe���쪺 Pointer �M�ᰵ�x�s
        /// </summary>
        private static Timer _PointerChecker { get; set; }


        /// <summary>
        /// �� �Ȧs�ƶq
        /// </summary>
        private readonly static int _SwapCount = 10;




        /// <summary>
        /// ��l�ư_�l�ƭ�
        /// </summary>
        private void InitPointerFromFile()
        {

            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "COUNT"))
            {

                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "COUNT", "1000");
                _IndexPointer = 1000;
            }
            else
            {

                var tmp = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "COUNT");
                _IndexPointer = int.Parse(tmp);
            }
        }




        /// <summary>
        /// �O�_���b�����ˬd
        /// �p�G���b������}
        /// </summary>
        private static bool IsRunningChecker { get; set; }



        /// <summary>
        /// Peek Now Pointer.
        /// </summary>
        /// <returns></returns>
        public static int PeekCurrentPointer()
        {

            int tmp = 0;
            while (_IndexPointerSwap.TryPeek(out tmp))
            {
                break;
            }

            return tmp;

        }


        /// <summary>
        /// ���s�Ұ��ˬd�� Timer
        /// </summary>
        public static void RestartTimerChecker()
        {

            _PointerChecker = new Timer();
            _PointerChecker.Elapsed += (sender, args) =>
            {

                if (IsRunningChecker) return;

                //simple lock for recyle.
                IsRunningChecker = true;

                int tmp = -1;
                //�ˬd�ثe������̨åB�ۼg�^ file.
                if (_IndexPointerSwap.TryPeek(out tmp))
                {
                    File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "COUNT", tmp.ToString());
                }


                IsRunningChecker = false;

            };

            _PointerChecker.Interval = 500;

            _PointerChecker.Start();


        }

        /// <summary>
        /// ��l�ƶ�J�h�֪�buffer pointer.
        /// </summary>
        /// <param name="num"></param>
        private void FillPointer(int num)
        {
            //���F�קK�����i��Q���e�ιL
            //�ҥH�����n�� ������ buffer �Ʃ���[�J
            var nP = _IndexPointer + 1 + _SwapCount;
            for (var i = (nP); i < (nP + num); i++)
            {
                _IndexPointerSwap.Enqueue(i);
            }

        }


        /// <summary>
        /// ���o�@�Ӽƭ�
        /// </summary>
        /// <returns></returns>
        public static int GetOneValue()
        {

            int res = -1;
            while (!_IndexPointerSwap.TryDequeue(out res))
            {

            }

            _IndexPointerSwap.Enqueue(res + _SwapCount);
            return res;
        }


        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {


            //SetPointer From File
            InitPointerFromFile();

            //Fille data to quqeue.
            _IndexPointerSwap = new System.Collections.Concurrent.ConcurrentQueue<int>();


            //Fill 100 to _IndexPointerSwap
            FillPointer(_SwapCount);

            //Start Timer
            RestartTimerChecker();


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
