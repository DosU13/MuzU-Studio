using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using MuzU;
using MuzU.data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace MuzU_Studio.Model
{
    internal class MidiImporter
    {
        internal static MuzUProject Import(Stream stream, string displayName, bool IsSimple)
        {
            MidiFile midiFile = MidiFile.Read(stream);
            MuzUProject result = new MuzUProject();
            MuzUData data = result.data;
            data.Name = displayName;
            TempoMap tempoMap = midiFile.GetTempoMap();
            long? microsecondPerQuarterNote = tempoMap.GetTempoChanges().LastOrDefault()?.Value?.MicrosecondsPerQuarterNote;
            string timeSignature = tempoMap.GetTimeSignatureChanges().LastOrDefault()?.Value?.ToString();
            CheckTempo(tempoMap);
            data.MicrosecondsPerQuarterNote = microsecondPerQuarterNote??Tempo.Default.MicrosecondsPerQuarterNote;
            data.TimeSignature = timeSignature??"4/4";
            data.TimingSequences.AddRange(ImportTimingSequences(midiFile, displayName, IsSimple));
            return result;
        }

        private static void CheckTempo(TempoMap tempoMap)
        {
            double mpq = tempoMap.GetTempoChanges().First().Value.MicrosecondsPerQuarterNote;
            bool isSingleTempo = true;
            foreach(ValueChange<Tempo> t in tempoMap.GetTempoChanges())
            {
                if (mpq != t.Value.MicrosecondsPerQuarterNote) isSingleTempo = false;
            }
            if (!isSingleTempo)
            {
                Debug.WriteLine("This midi file has multiple tempos: ");
                foreach (ValueChange<Tempo> t in tempoMap.GetTempoChanges()) Debug.WriteLine(t.Value.BeatsPerMinute);
            }
        }

        internal static List<TimingSequence> ImportTimingSequences(MidiFile midiFile, string displayName, bool IsSimple)
        {
            List<TimingSequence> result = new List<TimingSequence>();
            TempoMap tempoMap = midiFile.GetTempoMap();
            int timingSeqNameNumber = 1;
            foreach (TrackChunk trackChunk in midiFile.GetTrackChunks())
            {
                var trackNotes = trackChunk.GetNotes();
                if (trackNotes.Count > 0)
                {
                    TimingSequence timingSequence = new TimingSequence();
                    timingSequence.UniqueName = displayName + ((timingSeqNameNumber != 1) ? timingSeqNameNumber.ToString() : "");
                    timingSeqNameNumber++;
                    TimingTemplate template = new TimingTemplate();
                    template.LengthEnabled = true;
                    template.Properties = new List<TimingTemplateProperty>()
                        {new TimingTemplateProperty("NoteNumber", MuzU.data.ValueType.Integer)};
                    if (!IsSimple)
                    {
                        template.Properties.Add(new TimingTemplateProperty("Velocity", MuzU.data.ValueType.Integer));
                        template.Properties.Add(new TimingTemplateProperty("OffVelocity", MuzU.data.ValueType.Integer));
                    }
                    timingSequence.TimingTemplate = template;
                    foreach (Note note in trackNotes)
                    {
                        MetricTimeSpan metricTime = note.TimeAs<MetricTimeSpan>(tempoMap);
                        BarBeatTicksTimeSpan musicalTime = note.TimeAs<BarBeatTicksTimeSpan>(tempoMap);

                        MetricTimeSpan metricLength = note.LengthAs<MetricTimeSpan>(tempoMap);
                        int id = note.NoteNumber;

                        TimingItem item = new TimingItem();
                        item.Time = metricTime.TotalMicroseconds;
                        item.Length = metricLength.TotalMicroseconds;
                        item.Values = new List<double>() { (double)note.NoteNumber};
                        if (!IsSimple) { item.Values.Add((double)note.Velocity);
                                         item.Values.Add((double)note.OffVelocity); }
                        timingSequence.TimingItems.Add(item);
                    }
                    result.Add(timingSequence);
                }
            }
            return result;
        }

        private MuzUProject Project;
        private async Task BlendWithNewProject(MuzUProject newProject)
        {
            if (Project.data.Name == "NoName") Project.data.Name = newProject.data.Name;
            if (Project.data.MicrosecondsPerQuarterNote == null)
            {
                Project.data.MicrosecondsPerQuarterNote = newProject.data.MicrosecondsPerQuarterNote;
                Project.data.TimeSignature = newProject.data.TimeSignature;
            }
            else if (Project.data.MicrosecondsPerQuarterNote != newProject.data.MicrosecondsPerQuarterNote ||
                       Project.data.TimeSignature != newProject.data.TimeSignature)
                await AskForChoosingTempo(newProject.data.MicrosecondsPerQuarterNote, newProject.data.TimeSignature);
            Project.data.TimingSequences.AddRange(newProject.data.TimingSequences);
        }

        private async Task AskForChoosingTempo(long? microsecondsPerQuarterNote, string timeSignature)
        {
            if (Project.data.MicrosecondsPerQuarterNote == null || Project.data.TimingSequences == null)
            {
                Project.data.MicrosecondsPerQuarterNote = microsecondsPerQuarterNote;
                Project.data.TimeSignature = timeSignature;
            }
            else if (microsecondsPerQuarterNote != Project.data.MicrosecondsPerQuarterNote
               || Project.data.TimeSignature != timeSignature)
            {
                ContentDialog dialog = new ContentDialog();
                dialog.Title = "Tempo of midi not same as Project's tempo. Which one to use?";
                dialog.PrimaryButtonText = "Project";
                dialog.SecondaryButtonText = "Midi file";
                dialog.CloseButtonText = "Cancel";
                dialog.DefaultButton = ContentDialogButton.Primary;

                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary) return;
                else if (result == ContentDialogResult.Secondary)
                {
                    Project.data.MicrosecondsPerQuarterNote = microsecondsPerQuarterNote;
                    Project.data.TimeSignature = timeSignature;
                }
            }
        }
    }
}
