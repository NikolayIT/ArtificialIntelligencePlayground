﻿namespace DelaBgCourtDocumentsByDateAnomalyDetection
{
    using Microsoft.ML.Data;
    using System;

    public class ModelInput
    {
        [LoadColumn(0)]
        public DateTime DocumentDate { get; set; }

        [LoadColumn(1)]
        public double Count { get; set; }
    }
}