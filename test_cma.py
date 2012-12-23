import cma, numpy as np
from condor_util import Condor
#func = cma.Fcts.noisysphere
condor = Condor('/u/tansey/poker/evolve_vs_expert/experiments/experiment1', '/u/tansey/poker/evolve_vs_expert/evolve_vs_expert/bin/Release/evolve_vs_expert.exe')
es = cma.CMAEvolutionStrategy(np.zeros(1330), 1.5)
logger = cma.CMADataLogger().register(es)
for gens in range(100):
    print 'Getting population'
    X = es.ask()
    print 'Creating condor jobs'
    fit = np.array(condor.run_jobs(41, [25, 10], X))
    print 'Telling fitness. Min: {0}'.format(min(fit))
    es.tell(X, fit)  # prepare for next iteration
    #es.sigma *= nh(X, fit, func, es.ask)  # see method __call__
    #es.countevals += nh.evaluations_just_done  # this is a hack, not important though
    #logger.add(more_data = [nh.evaluations, nh.noiseS])  # add a data point
    #es.disp()
    # nh.maxevals = ...  it might be useful to start with smaller values and then increase
#print(es.stop())
#print(es.result()[-2])  # take mean value, the best solution is totally off
#assert sum(es.result()[-2]**2) < 1e-9
#print(X[np.argmin(fit)])  # not bad, but probably worse than the mean
logger.plot()
