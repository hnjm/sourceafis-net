using System;
using System.Collections.Generic;
using System.Text;
using SourceAFIS.Meta;
using SourceAFIS.General;

namespace SourceAFIS.Tuning.Optimization
{
    public sealed class MutationSequencer
    {
        public int MultipleAdvices = 10;
        public float ExtractorWeight = 0.2f;

        public delegate void MutationEvent(ParameterValue initial, ParameterValue mutated);
        public MutationEvent OnMutation;

        Random Random = new Random();

        public MutationAdvisor[] Advisors = new MutationAdvisor[] {
            new AxisFocusMutation(),
            new RandomMutation()
        };

        public ParameterSet Mutate(ParameterSet initial)
        {
            List<MutationAdvice> advices = new List<MutationAdvice>();
            foreach (MutationAdvisor advisor in Advisors)
                for (int i = 0; i < MultipleAdvices; ++i)
                    advices.AddRange(advisor.Advise(initial));
            
            AdjustExtractorWeight(advices);

            ParameterSet mutated = PickAdvice(advices).Mutated;

            string mutatedPath = mutated.GetDifference(initial).FieldPath;
            if (OnMutation != null)
                OnMutation(initial.Get(mutatedPath), mutated.Get(mutatedPath));

            return mutated;
        }

        public void Feedback(ParameterSet initial, ParameterSet mutated, bool improved)
        {
            foreach (MutationAdvisor advisor in Advisors)
                advisor.Feedback(initial, mutated, improved);
        }

        void AdjustExtractorWeight(List<MutationAdvice> advices)
        {
            foreach (MutationAdvice advice in advices)
            {
                ParameterValue parameter = advice.Mutated.GetDifference(advice.Initial);
                if (Calc.BeginsWith(parameter.FieldPath, "Extractor."))
                    advice.Confidence *= ExtractorWeight;
            }
        }

        MutationAdvice PickAdvice(List<MutationAdvice> advices)
        {
            float confidenceSum = 0;
            foreach (MutationAdvice advice in advices)
                confidenceSum += advice.Confidence;

            float randomWeight = (float)(Random.NextDouble() * confidenceSum);
            for (int i = 0; i < advices.Count; ++i)
            {
                randomWeight -= advices[i].Confidence;
                if (randomWeight < 0)
                    return advices[i];
            }
            return advices[advices.Count - 1];
        }
    }
}