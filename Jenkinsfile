pipeline {
    	
	agent any

	environment {
		PROJECT           = './Booth.DockerVolumeBackup.WebApi/Booth.DockerVolumeBackup.WebApi.csproj'
		TEST_PROJECT      = './Booth.DockerVolumeBackup.WebApi/Booth.DockerVolumeBackup.Test.csproj'
		PORTAINER_WEBHOOK = credentials('dockervolumebackup_webhook')
    }

    stages {
		stage('Build') {
			agent { 
				dockerfile { 
					filename 'JenkinsBuildAgentDockerFile' 
					reuseNode true
				}
			}

			stages {
				stage('Build') {
					steps {
						sh "dotnet build ${PROJECT} --configuration Release"
					}
				}

				stage('Test') {
					steps {
						sh "dotnet test ${TEST_PROJECT} --configuration Release --logger trx  --collect "XPlat Code Coverage" --results-directory ./testresults"
					}
				}

				stage('Publish') {
					steps {
						sh "dotnet publish ${PROJECT} --configuration Release --output ./deploy"
					}
				}
			}

		}
		
		stage('Deploy') {
			steps {
				script {
					def dockerImage = docker.build("craigbooth/dockervolumebackup")
					httpRequest httpMode: 'POST', responseHandle: 'NONE', url: "${PORTAINER_WEBHOOK}", wrapAsMultipart: false
				}
            }
		}
    }
	
	post {
		success {
			cleanWs()
		}
	}
}