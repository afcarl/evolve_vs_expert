import numpy as np
import os
from subprocess import call
import time

class Condor(object):

    def __init__(self, experiment_dir, exe_path):
        self.experiment_dir = experiment_dir
        
        if not self.experiment_dir.endswith('/'):
            self.experiment_dir = self.experiment_dir + '/'

        self.condor_file = experiment_dir + 'jobs'
        self.exe_path = exe_path
        self.genomes_dir = 'genomes/'
        self.results_dir = 'results/'
        self.finished_flags_dir = 'finished_flags/'
        self.champions_dir = 'champions/'
        self.output_dir = 'output/'
        self.condor_logs_dir = 'condor_logs/'
        self.error_dir = 'error/'
        self.user_group = 'GRAD'
        self.genomes_format = self.create_file_format(self.genomes_dir, 'genome_{0}.csv')
        self.results_format = self.create_file_format(self.results_dir, 'results_{0}.txt')
        self.finished_flags_format = self.create_file_format(self.finished_flags_dir, 'finished_{0}')
        self.champions_format = self.create_file_format(self.champions_dir, 'champion_{0}.csv')
        self.output_format = self.create_file_format(self.output_dir, 'output_{0}.out')
        self.condor_log_format = self.create_file_format(self.condor_logs_dir, 'job_{0}.log')
        self.error_format = self.create_file_format(self.error_dir, 'error_{0}.log')
        self.generations = 0


    def create_file_format(self, subdir, format):
        directory = self.experiment_dir + subdir
        if not os.path.exists(directory):
            os.makedirs(directory)
        return directory + format

    def run_jobs(self, inputs, hidden, candidate_weights):
        for filename in os.listdir(self.experiment_dir + self.finished_flags_dir):
            os.remove(self.experiment_dir + self.finished_flags_dir + filename)

        for i,weights in enumerate(candidate_weights):
            self.write_network(inputs, hidden, weights, self.genomes_format.format(i))

        self.create_condor_jobs(len(candidate_weights))
        self.submit_condor_jobs()
        self.wait_for_evaluations(len(candidate_weights))
        results = self.load_results(len(candidate_weights))
        self.save_champion(inputs, hidden, candidate_weights, results)
        self.generations += 1
        return results

    def write_network(self, inputs, hidden, weights, filename):
        f = open(filename, 'w')
        f.write('{0}\n'.format(inputs))
        f.write('{0}\n'.format(','.join([str(x) for x in hidden])))
        f.write('{0}\n'.format(','.join([str(x) for x in weights])))

    def create_condor_jobs(self, num_candidates):
        f = open(self.condor_file, 'w')
        f.write('universe = vanilla\n')
        f.write('Executable=/lusr/opt/mono-2.10.8/bin/mono\n')
        f.write('+Group   = "{0}"\n'.format(self.user_group))
        f.write('+Project = "AI_ROBOTICS"\n')
        f.write('+ProjectDescription = "Poker evolution experiments"\n')
        for i in range(num_candidates):
            f.write('Log = ' + self.condor_log_format.format(i) + '\n')
            f.write('Arguments = {0} {1} {2} {3}\n'.format(self.exe_path, self.genomes_format.format(i), self.results_format.format(i), self.finished_flags_format.format(i)))
            f.write('Output = ' + self.output_format.format(i) + '\n');
            f.write('Error = ' + self.error_format.format(i) + '\n')
            f.write('Queue 1')

    def submit_condor_jobs(self):
        call(["condor_submit", self.condor_file])

    def wait_for_evaluations(self, num_candidates):
        while(len(os.listdir(self.experiment_dir + self.finished_flags_dir)) < num_candidates):
            time.sleep(1)

    def load_results(self, num_candidates):
        results = []
        for i in range(num_candidates):
            f = open(self.results_format.format(i), 'r')
            results.append(float(f.readline()))
        return results

    def save_champion(self, inputs, hidden, candidate_weights, results):
        max_score = results[0]
        champion = candidate_weights[0]
        for i,w in enumerate(candidate_weights):
            if(results[i] > max_score):
                max_score = results[i]
                champion = w
        self.write_network(inputs, hidden, champion, self.champions_format.format(self.generations))
