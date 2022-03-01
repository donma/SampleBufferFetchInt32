using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SampleBufferFetchInt32.Pages
{
    public class IndexModel : PageModel
    {
     
   
        static HashSet<string> _checkDup { get; set; }

        public string Context { get; set; }
        public void OnGet()
        {


        }

        public IActionResult OnPostSubmit()
        {
     
            Stopwatch st = new Stopwatch();
            st.Start();

            ConcurrentBag<string> _tmp = new ConcurrentBag<string>();

            Parallel.For(0, 1_000, i =>
            {
                var ts = Startup.GetOneValue();
                _tmp.Add(ts.ToString());
              
            });


            st.Stop();

            //檢查有沒有重複
            _checkDup = new HashSet<string>();
            foreach (var c in _tmp)
            {
                _checkDup.Add(c);

            }

            Context += _checkDup.Count + "," + _tmp.Count + "," +Startup.PeekCurrentPointer()+ "," + st.Elapsed + "<br>";

            return Page();
        }
    }
}
