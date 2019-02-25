using UnityEngine;

namespace VARP.KochFractals
{
    [RequireComponent(typeof(AudioSource))] 
    public class AudioPeer : MonoBehaviour
    {
        /// <summary>
        /// takes all the hertz in the total spectrum of the audio playing, 20000 samples, and put´s them into 512 samples
        /// </summary>
        public readonly float[] samples = new float[512]; 
        public readonly float[] freqBand = new float[8];
        public readonly float[] bandBuffer = new float[8];
        public readonly float[] bufferDecrease = new float[8];
        public readonly float[] audioBand = new float[8];
        public readonly float[] audioBandBuffer = new float[8];
        
        private AudioSource audioSource;
        private readonly float[] freqBandHighest = new float[8];
        
        public float amplitude;
        public float amplitudeBuffer;
        public float amplitudePeak;
        private const float PEAK_DECREASING_RATIO = 0.98f;
        
        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
        }

        private void Update()
        {
            GetSpectrumAudioSource();
            MakeFrequencyBands();
            BandBuffer();
            CreateAudioBands();
            GetAmplitude();
        }
        
        /// <summary>
        /// divides the 512 samples into eight frequency bands
        /// </summary>
        private void MakeFrequencyBands() 
        {
            /*
             * 22050 / 512 = 43Hz per sample
             *   20 -    60Hz
             *   60 -   250Hz
             *  250 -   500Hz
             *  500 -  2000Hz
             * 2000 -  4000Hz
             * 4000 -  6000Hz
             * 6000 - 20000Hz
             *
             * 0 -   2 = 86Hz
             * 1 -   4 = 172Hz
             * 2 -   8 = 344Hz
             * 3 -  16 = 688Hz
             * 4 -  32 = 1376Hz
             * 5 -  64 = 2752Hz
             * 6 - 128 = 5504Hz
             * 7 - 256 = 11008Hz
             * 510
             */
                
            var count = 0;
            for (var i = 0; i < 8; i++)
            {
                float average = 0;
                var sampleCount = (int) Mathf.Pow(2, i) * 2;
                if (i == 7) sampleCount += 2;
                for (var j = 0; j < sampleCount; j++)
                {
                    average += samples[count] * (count + 1);
                    count++;
                }
                average /= count;
                freqBand[i] = average * 10;
            }
        }

        /// <summary>
        /// takes audio sources spectrum data and puts them into samples
        /// </summary>
        private void GetSpectrumAudioSource()
        {
            audioSource.GetSpectrumData(samples, 0, FFTWindow.Blackman); 
        }

        /// <summary>
        /// buffer to the value which creates a smooth down when the amplitude is lower than its previous value
        /// </summary>
        private void BandBuffer() 
        {
            for (var g = 0; g < 8; ++g)
            {
                if (freqBand[g] > bandBuffer[g])
                {
                    bandBuffer[g] = freqBand[g];
                    bufferDecrease[g] = 0.005f;
                }

                if (freqBand[g] < bandBuffer[g])
                {
                    bandBuffer[g] -= bufferDecrease[g];
                    bufferDecrease[g] *= 1.2f;
                }
            }
        }
        
        /// <summary>
        /// create values between zero and one that can be applied to a lot of different outputs
        /// </summary>
        private void CreateAudioBands()
        {
            for (var i = 0; i < 8; i++)
            {
                if (freqBand[i] > freqBandHighest[i]) freqBandHighest[i] = freqBand[i];
                audioBand[i] = freqBand[i] / freqBandHighest[i];
                audioBandBuffer[i] = bandBuffer[i] / freqBandHighest[i];
            }
        }

        private void GetAmplitude()
        {
            var currentAmplitude = 0f;
            var currentAmplitudeBuffer = 0f;
            for (var i=0 ; i < 8; i++)
            {
                currentAmplitude += audioBand[i];
                currentAmplitudeBuffer += audioBandBuffer[i];
            }

            if (currentAmplitude > amplitudePeak)
                amplitudePeak = currentAmplitude;
            else
                amplitudePeak *= PEAK_DECREASING_RATIO;
            if (amplitudePeak == 0)
            {
                amplitude = amplitudeBuffer = 0f;
            }
            else
            {
                amplitude = currentAmplitude / amplitudePeak;
                amplitudeBuffer = currentAmplitudeBuffer / amplitudePeak;
            }
        }
    }
}