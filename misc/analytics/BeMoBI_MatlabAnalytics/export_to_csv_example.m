
% delay in meinen marker finden: mit der hand 20 marker und die werte
% rausschreiben und dann mitteln und von allen abziehen um die beiden
% zeitreihen zu alignen

% Behavioral Data Preprocessing

subject = 1;

%anpassen
movement = 'optFlow';
%movement = 'physical';


%% 1
% create structs for shoulders, events, dof out of newstream object
for i = 1:3
    if strcmp(newstream{1,i}.info.name,'TrackingShoulders')
        shoulders = newstream{1,i};
    elseif strcmp(newstream{1,i}.info.name,'Events')
        events = newstream{1,i};
    else
        dof = newstream{1,i};
    end
end

%% 2
% Interessante Marker Trial Start - End Target Stimulus; Onset Back
% Rotation - End Back Rotation

% Marker of current Interest
markerOfInterest = {'End Target Stimulus', 'End Back rotation', 'Onset Target Stimulus'};

tmpMarkerTime = [];
tmpMarkerTime2 = [];
tmpMarkerTime3 = [];

markerTime = [];
idxPos = [];
accuracy = [];
hinrotation = [];
closest = [];
countMarkers = 0;

% array for currentElem, idxPos, Accuracy, RT

allInterest = cell(136,9);

% Stimuli list as presented in experiment
tmpStimuli = cell(136,1);


% find time_stamps of Marker of given "markerOfInterest" and create array 
% of all the occurences; find closest value of a certain marker position
for i = 1:length(events.time_series)
    
    % find a certain Marker
    if strcmp(events.time_series(i),markerOfInterest(2))
        countMarkers = countMarkers + 1; %control, count should be 136
        tmpMarkerTime(i) = events.time_stamps(i);
    end
    
    % find another Marker, to calculate RT
    if strcmp(events.time_series(i),markerOfInterest(1))
        tmpMarkerTime2(i) = events.time_stamps(i);
    end
    
    % find another Marker, to get Stimulus
    if strcmp(events.time_series(i),markerOfInterest(3))
        tmpMarkerTime3(i) = events.time_stamps(i);
        tmpStimuli(i) = events.time_series(i-2);
    end
    
end

% throw out all zero Values of non-marker Positions
markerTime = tmpMarkerTime(find(tmpMarkerTime~=0));
markerTime2 = tmpMarkerTime2(find(tmpMarkerTime2~=0));
markerTime3 = tmpMarkerTime3(find(tmpMarkerTime3~=0));

stimulus = tmpStimuli(~cellfun('isempty',tmpStimuli));

% get accuracy measure out of 6dof.time_series
for i = 1:numel(markerTime)
    currentElem = markerTime(i);
    tmp = abs(dof.time_stamps-currentElem);
    [idx idx] = min(tmp); %index of closest value
    closest(i) = dof.time_stamps(idx); %closest value
    idxPos(i) = idx;
    
    % average 3 samples for accuracy measure, due to slight drift in
    % sampled data
    % mean of the next 3 values is goal value, create array and take the mean
    % of the kernel values
    %kernel = [idx-4, idx-3, idx-2];
    % how to best define accuracy measure?
    %accuracy(i) = mean(dof.time_series(1,kernel));
    
    accuracy(i) = mean(dof.time_series(1,idx));
    
    tmp = strsplit(stimulus{i},',');
    tmpNum = str2num(tmp{1,2});
    allInterest{i,9} = tmpNum*-1;
    
    % wie automatisieren?
    allInterest{i,1} = movement;
    allInterest{i,5} = accuracy(i);
    allInterest{i,6} = abs(accuracy(i));
    
    % sort through stimuli
    if ~isempty(strfind(tmp{1},'(''clock''')) % clock trial
        
        allInterest{i,2} = 'clock';
        
        if ~isempty(strfind(tmp{3},'''slow'')')) % slow trial
                
            allInterest{i,3} = 'slow';
            
            % trotzdem drinlassen
            if tmpNum <= -30 && tmpNum >= -60
                allInterest{i,4} = -45;
            elseif tmpNum <= -75 && tmpNum >= -105
                allInterest{i,4} = -90;
            elseif tmpNum <= -120 && tmpNum >= -150
                allInterest{i,4} = -135;
            elseif tmpNum <= -165 && tmpNum >= -195
                allInterest{i,4} = -180;
            elseif tmpNum == -15 || -68 || -113 || -158
                allInterest{i,4} = 0;
            end
        else % if not slow then fast
            
            allInterest{i,3} = 'fast';
            
            % trotzdem drinlassen
            if tmpNum <= -30 && tmpNum >= -60
                allInterest{i,4} = -45;
            elseif tmpNum <= -75 && tmpNum >= -105
                allInterest{i,4} = -90;
            elseif tmpNum <= -120 && tmpNum >= -150
                allInterest{i,4} = -135;
            elseif tmpNum <= -165 && tmpNum >= -195
                allInterest{i,4} = -180;
            elseif tmpNum == -15 || -68 || -113 || -158
                allInterest{i,4} = 0;
            end            

        end
    else % if not clock then anticlock
        
        allInterest{i,2} = 'anticlock';
        
        if ~isempty(strfind(tmp{3},'''slow'')')) % slow trial
            
            allInterest{i,3} = 'slow';
            
            if tmpNum >= 30 && tmpNum <= 60
                allInterest{i,4} = 45;
            elseif tmpNum >= 75 && tmpNum <= 105
                allInterest{i,4} = 90;
            elseif tmpNum >= 120 && tmpNum <= 150
                allInterest{i,4} = 135;
            elseif tmpNum >= 165 && tmpNum <= 195
                allInterest{i,4} = 180;
            elseif tmpNum == 15 || 68 || 113 || 158
                allInterest{i,4} = 0;
            end
        else % if not slow then fast
            
            allInterest{i,3} = 'fast';
            
            if tmpNum >= 30 && tmpNum <= 60
                allInterest{i,4} = 45;
            elseif tmpNum >= 75 && tmpNum <= 105
                allInterest{i,4} = 90;
            elseif tmpNum >= 120 && tmpNum <= 150
                allInterest{i,4} = 135;
            elseif tmpNum >= 165 && tmpNum <= 195
                allInterest{i,4} = 180;
            elseif tmpNum == 15 || 68 || 113 || 158
                allInterest{i,4} = 0;
            end
        end
    end

%     allInterest(i,1) = currentElem;
%     allInterest(i,2) = idxPos(i);
%     allInterest(i,3) = accuracy(i);
    
    % get RT measure
    elem2 = markerTime2(i);
    rt = currentElem - elem2;
    allInterest{i,7} = rt;
    allInterest{i,10} = stimulus{i};
end 

for i = 1:numel(markerTime2)
    currentElem = markerTime2(i);
    tmp = abs(dof.time_stamps-currentElem);
    [idx idx] = min(tmp); %index of closest value
    closest(i) = dof.time_stamps(idx); %closest value
    idxPos(i) = idx;
    
    hinrotation(i) = dof.time_series(1,idx);

    if abs(accuracy(i)-hinrotation(i))>120;
        allInterest{i,8} = 'Outlier';
    else
        allInterest{i,8} = hinrotation(i);
    end

end

%% 3
% Maße bestimmen

% % All error (Trials only)
% allErrors = accuracy(find(accuracy~=0));
% 
% % Relative errors: Positive error und negative error
% positiveError = accuracy(accuracy>0);
% negativeError = accuracy(accuracy<0); 
% 
% % Absolute Error
% absoluteError = abs(accuracy);

% Stimulus
% stimulus = tmpStimuli(~cellfun('isempty',tmpStimuli));


%% 4
% save to spss/excel, save stimulus triallist separate

% clock, slow, 45...

% anpassen
filename = strcat('behavior_subject',subject);
% xlwrite(filename,allInterest);
csvwrite(filename,allInterest);




