﻿using PPTCombiner.FS;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

namespace PPTCombiner.Commands
{
    class PerformMerge : ICommand
    {
        //TODO: Change to PPTCombiner.FS.AddedPath
        private readonly ObservableCollection<AddedPath> addedPaths;

        public PerformMerge(ObservableCollection<AddedPath> addedPaths)
        {
            this.addedPaths = addedPaths;
            this.addedPaths.CollectionChanged += addedPaths_CollectionChanged;
        }

        void addedPaths_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CanExecuteChanged.Raise(this, EventArgs.Empty);
        }

        public bool CanExecute(object parameter)
        {
            int validMergeTargets = addedPaths.SelectMany(PathHelpers.GetMergeTargets).Count();
            return validMergeTargets > 0;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            var validFiles = addedPaths
                .SelectMany(PathHelpers.GetMergeTargets)
                .Select(x => x.AddedPath);
            Type pptType = Type.GetTypeFromProgID("Powerpoint.Application");  
          
            if(pptType == null)
            {
                MessageBox.Show("Could not launch Microsoft Powerpoint (Powerpoint.Application). Powerpoint may not be installed correctly (or at all!) if you are setting this error message.");
            }
            else
            {
                dynamic pptApp = Activator.CreateInstance(pptType);
                pptApp.Visible = true;
                dynamic presentation = pptApp.Presentations.Add();

                foreach (string validFile in validFiles)
                {
                    int slideCount = presentation.Slides.Count;
                    try
                    {
                        presentation.Slides.InsertFromFile(validFile, slideCount);
                    }
                    catch (COMException e)
                    {
                        Debug.Write(e.InnerException);
                    }

                }
            }
        }
    }
}
