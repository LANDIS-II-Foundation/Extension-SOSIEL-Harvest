# What is the SOSIEL Harvest extension (SHE)?

The SOSIEL (pronounced ˈsōSHəl and stands for Self-Organizing Social & Inductive Evolutionary Learning) Harvest extension (SHE) implements boundedly-rational decision-making by one or more agents. Each SOSIEL agent makes decisions using a cognitive architecture that consists of nine cognitive processes that enable each agent to interact with other agents, learn from its own experience and that of others, and make decisions about taking, and then take, (potentially collective) actions. Together, LANDIS-II and SHE have the potential to simulate adaptive management in co-evolving coupled human and forest landscapes. 

# Features

- Each agent can engage in anticipatory learning, goal prioritizing, counterfactual thinking, innovating, social learning, goal selecting, satisficing, signaling, and (potentially collective) action-taking.
- Four alternative cognitive levels, with each level composed of a different combination of cognitive processes and representing an alternative approach to modeling agent behavior.
- Three alternative modes: Mode 1 is primarily intended for simulating site-scale forest management by mobile agents, Mode 2 is intended for simulating stand- to landscape-scale forest management, and Mode 3 simulates agents that do not directly interact with the forest landscape.

# Release Notes

- Latest official release: Version 1.1.14 — February 2021
- [SHE User Guide](https://docs.google.com/document/d/1YBKuFaQ5Hsh3OjYsMJoXoHgtg7gv8Us0wZjcTaqSCOc).

# Requirements

To use SHE, you need:

- The [LANDIS-II model v8.0](http://www.landis-ii.org/install) installed on your computer.
- SHE and, for use in Mode 2, [Biomass Harvest](https://sites.google.com/site/landismodel/extensions) installed on your computer.
- Input files for LANDIS-II; SHE; and, for use in Mode 2, Biomass Harvest. An examples of files is [here]( https://github.com/LANDIS-II-Foundation/Project-Michigan-Compare-Harvesting-2021).

# Download

Version 1.1.14 can be downloaded [here](https://github.com/LANDIS-II-Foundation/Extension-SOSIEL-Harvest/blob/master/deploy/installer/LANDIS-II-V7%20SOSIEL%20Harvest%201.1.14-setup.exe). To install it on your computer, just launch the installer.

# Citation

When using SHE in Mode 1 or 3:
- Cite the manuscript describing SHE's Mode 1 and 3: Sotnik, G., Kovalchuk, K., Pizhenko, I., Thom, D., Kruhlov, I., Chaskovskyy, O., Nielsen-Pincus, M., & Scheller, R. M. (in preparation) A new agent-based model simulates human-forest-wildlife co-evolution in the Ukrainian Carpathians.
- Cite the manuscript describing the SOSIEL algorithm: Sotnik, G. (2018). [The SOSIEL Platform: Knowledge-based, cognitive, and multi-agent](https://www.sciencedirect.com/science/article/abs/pii/S2212683X18301038). _Biologically Inspired Cognitive Architectures, 26_, 103-117. https://doi.org/10.1016/j.bica.2018.09.001

When using SHE in Mode 2:
- Cite the manuscript describing SHE's Mode 2: Sotnik, G., Cassell, B. A., Duveneck, M. J., Scheller, R. M. (2021) [A new agent-based model provides insight into deep uncertainty faced in simulated forest management](https://doi.org/10.1007/s10980-021-01324-5). _Landscape Ecology_. https://doi.org/10.1007/s10980-021-01324-5
- Cite the manuscript describing the Biomass Harvest extension: Gustafson, E. J., Shifley, S. R., Mladenoff, D. J., Nimerfro, K. K., & He, H. S. (2000). [Spatial simulation of forest succession and timber harvesting using LANDIS](https://www.fs.usda.gov/treesearch/pubs/12076). _Canadian Journal of Forest Research, 30_(1), 32–43. https://doi.org/10.1139/x99-188
- Cite the manuscript describing the SOSIEL algorithm: Sotnik, G. (2018). [The SOSIEL Platform: Knowledge-based, cognitive, and multi-agent](https://www.sciencedirect.com/science/article/abs/pii/S2212683X18301038?via%3Dihub). _Biologically Inspired Cognitive Architectures, 26_, 103-117. https://doi.org/10.1016/j.bica.2018.09.001

# Support

If you have a question, contact Garry Sotnik at contact@sosiel.org. 
You can also ask for help in the [LANDIS-II users group](http://www.landis-ii.org/users).

If you come across any issue or suspected bug when using SHE, contact Garry Sotnik at contact@sosiel.org or post about it in the [issue section of the GitHub repository](https://github.com/LANDIS-II-Foundation/Extension-SOSIEL-Harvest/issues).

# Design team

- Mode 1: Garry Sotnik
- Mode 2: Garry Sotnik, Brooke A. Cassell, & Robert M. Scheller
- Mode 3: Garry Sotnik

# Development team

- Mode 1: Ivan Pizhenko, Vadim Moskvin, Garry Sotnik, & Eugene Lobach
- Mode 2: Vadim Moskvin, Garry Sotnik, & Eugene Lobach
- Mode 3: Ivan Pizhenko & Garry Sotnik
