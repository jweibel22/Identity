﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Domain
{
    public class RssFeeder
    {
        public long Id { get; set; }

        public string Url { get; set; }

        public DateTime? LastFetch { get; set; }
    }
}
