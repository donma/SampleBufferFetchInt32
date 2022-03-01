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

        //Buffer 使用
        private static System.Collections.Concurrent.ConcurrentQueue<Int32> _IndexPointerSwap;

        private static int _IndexPointer;

        /// <summary>
        /// 檢查目前走到的 Pointer 然後做儲存
        /// </summary>
        private static Timer _PointerChecker { get; set; }


        /// <summary>
        /// Ｑ 暫存數量
        /// </summary>
        private readonly static int _SwapCount = 10;




        /// <summary>
        /// 初始化起始數值
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
        /// 是否正在執行檢查
        /// 如果正在執行跳開
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
        /// 重新啟動檢查的 Timer
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
                //檢查目前走到哪裡並且抄寫回 file.
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
        /// 初始化填入多少的buffer pointer.
        /// </summary>
        /// <param name="num"></param>
        private void FillPointer(int num)
        {
            //為了避免中間可能被之前用過
            //所以必須要用 中間的 buffer 數往後加入
            var nP = _IndexPointer + 1 + _SwapCount;
            for (var i = (nP); i < (nP + num); i++)
            {
                _IndexPointerSwap.Enqueue(i);
            }

        }


        /// <summary>
        /// 取得一個數值
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
