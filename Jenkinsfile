pipeline {
    	
	agent any

	environment {
		PROJECT      = './Booth.DockerTest/Booth.DockerTest.csproj'
    }

    stages {
		stage('Build') {
			agent { 
				docker { 
					image 'mcr.microsoft.com/dotnet/sdk:5.0-alpine' 
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
					httpRequest httpMode: 'POST', responseHandle: 'NONE', url: 'https://portainer.boothfamily.id.au/api/webhooks/b2cbc165-1a43-499f-ac3b-058146abf907', wrapAsMultipart: false
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