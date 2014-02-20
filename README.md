Physiovalues
============

Project of web application to compute, store and provide values of parameters of physiological models via lightweight API. 

The system supports modeling of physiological systems in the phase of calibrating model parameters and in the phase of simulating different scenarios. The loosely coupled part of the system is deployed in a remote distributed computational capacity. A significant speedup was shown in the case of the large complex physiological model computed in cloud computing infrastructure provided by Czech NGI (CESNET) resources. The system capabilities is accessible via a web application and allows user to focus on experimental data, names of parameters, visual control of the calibration computation and hide unnecessary complexity of the remote subsystems computation. The data of real experiments and simulations are stored and provided for further research.

The web application for calibration of model integrates several technologies. The computational demanding process is designed as master worker. The identification algorithm employs the well known genetic algorithm implemented in MATLAB environment and exported as DLL library. The server (master) module manages the identification algorithm and distributes single simulation tasks to workers. Workers provides a REST interface to receive simulation task requests. We exported the models of Human physiology from Modelica langugage into standardized FMU package which is in fact DLL library with standardized API on Windows platform. The client application is developed in HTML5 utilizing AJAX technology to communicate with server services, collect data and request computation.

