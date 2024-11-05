pipeline {
    	
	agent any

	environment {
		PROJECT           = './Booth.DockerVolumeBackup.WebApi/Booth.DockerVolumeBackup.WebApi.csproj'
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