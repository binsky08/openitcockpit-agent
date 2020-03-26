pipeline {
    agent any
    
    stages {
        stage('Build agent linux packages') {
            when {
                beforeAgent true
                branch 'master'
            }
            agent {
                docker { 
                    image 'srvitsmdrone01.master.dns:5000/openitcockpit-agent-centos7:latest'
                    registryUrl 'http://srvitsmdrone01.master.dns:5000'
                }
            }
            steps {
                sh 'yum -y install python36-devel python36-pip libffi-devel gcc glibc ruby-devel make rpm-build rubygems rpm bsdtar'
                sh 'gem install --no-ri --no-rdoc fpm'
                sh 'mkdir -p ./public/{packages,binaries}'
                sh 'mkdir -p ./release'
                
                sh 'pip3 install -r requirements.txt'
                sh 'pyinstaller oitc_agent.py --onefile'
                
                sh 'mv ./dist/oitc_agent ./public/binaries/openitcockpit-agent-python3.linux.bin'
                sh 'chmod +x ./public/binaries/openitcockpit-agent-python3.linux.bin'
                sh '/bin/cp -f ./public/binaries/openitcockpit-agent-python3.linux.bin executables'
                sh './packages/scripts/build_linux_ci.sh'
                sh 'mv openitcockpit-agent*.{deb,rpm,pkg.tar.xz} ./release'
                archiveArtifacts artifacts: 'release/**', fingerprint: true
                script {
                    stash includes: 'release/**', name: 'release'
                }
            }
        }
        stage('Publish linux packages') {
            when {
                beforeAgent true
                branch 'master'
            }
            agent {
                docker { 
                    image 'srvitsmdrone01.master.dns:5000/alpine-rsync'
                    registryUrl 'http://srvitsmdrone01.master.dns:5000'
                }
            }
            environment {
                SSH_KEY = credentials('JENKINS_SSH_KEY')
                VERSION = """${sh(
                    returnStdout: true,
                    script: 'cat version | xargs | tr -d \'\\n\''
                )}"""
            }
            steps {
                script {
                    unstash 'release'
                }
                sh 'ssh -o StrictHostKeyChecking=no -o UserKnownHostsFile=/dev/null -i $SSH_KEY root@172.17.0.1 "mkdir -p /tmp/agent/test; rm -r /tmp/agent/*"'
                sh 'rsync -avz -e "ssh -o StrictHostKeyChecking=no -o UserKnownHostsFile=/dev/null -i $SSH_KEY" --progress release/* root@172.17.0.1:/tmp/agent'
                sh 'ssh -o StrictHostKeyChecking=no -o UserKnownHostsFile=/dev/null -i $SSH_KEY root@172.17.0.1 "cd /root/openITCOCKPIT-build; ./aptly.sh repo add -force-replace buster-agent-nightly /tmp/agent/openitcockpit-agent_*amd64.deb; ./aptly.sh repo add -force-replace stretch-agent-nightly /tmp/agent/openitcockpit-agent_*amd64.deb; ./aptly.sh repo add -force-replace bionic-agent-nightly /tmp/agent/openitcockpit-agent_*amd64.deb; ./aptly.sh repo add -force-replace xenial-agent-nightly /tmp/agent/openitcockpit-agent_*amd64.deb"'
                sh 'ssh -o StrictHostKeyChecking=no -o UserKnownHostsFile=/dev/null -i $SSH_KEY oitc@172.16.101.32 "mkdir -p /var/www/openitcockpit_io/files/openitcockpit-agent"'
                sh 'rsync -avz -e "ssh -o StrictHostKeyChecking=no -o UserKnownHostsFile=/dev/null -i $SSH_KEY" --progress release/* oitc@172.16.101.32:/var/www/openitcockpit_io/files/openitcockpit-agent'
                sh 'ssh -o StrictHostKeyChecking=no -o UserKnownHostsFile=/dev/null -i $SSH_KEY root@172.16.101.113 "mkdir -p /var/repositories/openitcockpit-agent/${VERSION}"'
                sh 'rsync -avu -e "ssh -o StrictHostKeyChecking=no -o UserKnownHostsFile=/dev/null -i $SSH_KEY" --delete --progress release/* root@172.16.101.113:/var/repositories/openitcockpit-agent/${VERSION}'
            }
        }
        stage('Build agent windows packages') {
            when {
                beforeAgent true
                branch 'master'
            }
            agent {
                docker { 
                    image 'srvitsmdrone01.master.dns:5000/alpine-rsync'
                    registryUrl 'http://srvitsmdrone01.master.dns:5000'
                }
            }
            environment {
                SSH_KEY = credentials('JENKINS_SSH_KEY')
                VERSION = """${sh(
                    returnStdout: true,
                    script: 'cat version | xargs | tr -d \'\\n\''
                )}"""
            }
            steps {
                sh 'scp -o StrictHostKeyChecking=no -o UserKnownHostsFile=/dev/null -i $SSH_KEY -r ./ kress@172.16.166.223:openitcockpit-agent'
                sh 'ssh -o StrictHostKeyChecking=no -o UserKnownHostsFile=/dev/null -i $SSH_KEY kress@172.16.166.223 powershell "cd openitcockpit-agent; python.exe -m venv ./python3-windows-env; ./python3-windows-env/Scripts/activate.bat; ./python3-windows-env/Scripts/pip.exe install -r requirements.txt servicemanager; ./python3-windows-env/Scripts/pyinstaller.exe oitc_agent.py --onefile; ./python3-windows-env/Scripts/deactivate.bat"'
                sh 'ssh -o StrictHostKeyChecking=no -o UserKnownHostsFile=/dev/null -i $SSH_KEY kress@172.16.166.223 powershell "cd openitcockpit-agent; rm -fo executables/openitcockpit-agent-python3.exe; mv ./dist/oitc_agent.exe executables/openitcockpit-agent-python3.exe; rm -r -fo ./dist; rm -r -fo ./build; rm -r -fo ./__pycache__; rm -r -fo ./oitc_agent.spec; rm -r -fo ./python3-windows-env"'
                sh 'ssh -o StrictHostKeyChecking=no -o UserKnownHostsFile=/dev/null -i $SSH_KEY kress@172.16.166.223 powershell "openitcockpit-agent/packages/scripts/build_msi.bat"'
                sh 'ssh -o StrictHostKeyChecking=no -o UserKnownHostsFile=/dev/null -i $SSH_KEY kress@172.16.166.223 powershell "Rename-Item -Path openitcockpit-agent/msi/openitcockpit-agent.msi openitcockpit-agent-${VERSION}.msi"'
                sh 'mkdir -p ./release'
                sh 'scp -o StrictHostKeyChecking=no -o UserKnownHostsFile=/dev/null -i $SSH_KEY kress@172.16.166.223:openitcockpit-agent/msi/openitcockpit-agent-${VERSION}.msi ./release'
                sh 'ssh -o StrictHostKeyChecking=no -o UserKnownHostsFile=/dev/null -i $SSH_KEY kress@172.16.166.223 powershell "rm -r -fo openitcockpit-agent"'
                archiveArtifacts artifacts: 'release/openitcockpit-agent-${VERSION}.msi', fingerprint: true
                script {
                    stash includes: 'release/**', name: 'windowsrelease'
                }
            }
        }
        stage('Publish windows package') {
            when {
                beforeAgent true
                branch 'master'
            }
            agent {
                docker { 
                    image 'srvitsmdrone01.master.dns:5000/alpine-rsync'
                    registryUrl 'http://srvitsmdrone01.master.dns:5000'
                }
            }
            environment {
                SSH_KEY = credentials('JENKINS_SSH_KEY')
                VERSION = """${sh(
                    returnStdout: true,
                    script: 'cat version | xargs | tr -d \'\\n\''
                )}"""
            }
            steps {
                script {
                    unstash 'windowsrelease'
                }
                sh 'ssh -o StrictHostKeyChecking=no -o UserKnownHostsFile=/dev/null -i $SSH_KEY oitc@172.16.101.32 "mkdir -p /var/www/openitcockpit_io/files/openitcockpit-agent"'
                sh 'rsync -avz -e "ssh -o StrictHostKeyChecking=no -o UserKnownHostsFile=/dev/null -i $SSH_KEY" --progress release/* oitc@172.16.101.32:/var/www/openitcockpit_io/files/openitcockpit-agent'
                sh 'ssh -o StrictHostKeyChecking=no -o UserKnownHostsFile=/dev/null -i $SSH_KEY root@172.16.101.113 "mkdir -p /var/repositories/openitcockpit-agent/${VERSION}"'
                sh 'rsync -avu -e "ssh -o StrictHostKeyChecking=no -o UserKnownHostsFile=/dev/null -i $SSH_KEY" --progress release/* root@172.16.101.113:/var/repositories/openitcockpit-agent/${VERSION}'
            }
        }
        stage('Nothing done') {
            when {
                beforeAgent true
                not {
                    branch 'master'
                }
            }
            steps {
                echo 'Nothing done'
            }
        }
    }
}
 
