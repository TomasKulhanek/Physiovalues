function [x, fval] = ANNI(Fname)
% Solver of the set of non-linear equations
% *****************************************************************************
% Artificial
% Neural
% Network
% Inversion
%
% For detailed description of the algorithm see the file ANNI_EN.pdf
% *****************************************************************************
% On input: Fname = name of the file containing full description of the
% problem to be solved
% On output: x = vector of parameters (solution to the problem)
%            fval = norm of the RHS residuals
% ==============
% Important note
% ==============
% After starting/finishing 'ANNI' program please look carefully into the file 'ANNI.log',
% which collects all information regarding the inverse process. 'ANNI.log' is opened in
% append mode in working directory any time ANNI has been started.
%
% January 2010  Co.Bohuslav Ruzek
% *****************************************************************************
global ParDef

VER = '1.3';

ParDef = [];
Popul  = [];
RBFN   = [];
x      = [];
fval   = [];

fd_log = -1;
ParDef.err = 0;
read_param(Fname);
if(ParDef.err == 0)
    ShrinkPar(+1);
    init();
    [ib, fb] = EvalPop();
    gb = fb;
    log_inv('');
    d0  = zeros(ParDef.n,1);
    nwait = 0;
    nstop = 0;
    while(nstop == 0)
        nstop = stop_test();
        if(nstop ~= 0)
            RBFN.t_i = RBFN.t_i + toc();
            tic();
            eval(sprintf('%s(3, StretchPar(Popul.Par(:,Popul.ind(1))));', ParDef.fcn));
            RBFN.t_e = RBFN.t_e + toc();
            tic();
            Popul.sol = Popul.sol + 1;
            Popul.PP(:,Popul.sol) = Popul.Par(:,Popul.ind(1));
            Popul.DD(:,Popul.sol) = Popul.Dat(:,Popul.ind(1));
            Popul.CHI(Popul.sol)  = TrueFit(Popul.ind(1));
            dsol = IndexSol();
            if((Popul.msl < ParDef.msol) && (Popul.sol < ParDef.msol*2))
                reinit();
                if(dsol ~= 0)
                    Popul.enc = Popul.enc*2;
                    %                    fprintf(1,'Duplicated estimated convergency rate %d\n',Popul.enc);
                end
                gb = TrueFit(Popul.ind(Popul.q));
                nstop = 0;
            else
                continue;
            end
        end
        % S E L E C T I O N    B L O C K
        Select(0, 'R', RBFN.R);
        Select(RBFN.cen, 'G', 0);
        ns  = Popul.qq - 1;
        rc2 = (RBFN.R/10)^2;
        i   = 0;
        nxx = 0;
        nxc = ns*10;
        while(i ~= ns)
            nxx = nxx + 1;
            if(nxx == nxc)
                break;
            end
            xpar = rand(ParDef.M,1)-0.5;
            if(ParDef.M > 1)
                xmag = norm(xpar);
                if(xmag == 0)
                    continue;
                end
                xpar = xpar/norm(xpar);
            end
            xpar = RBFN.R*Popul.Cl.*xpar+RBFN.cen;
            xpar = adjust(xpar,RBFN.cen);
            if(dist_p2(RBFN.cen, xpar) < rc2)
                %    fprintf(1, 'x\n');
                continue;
            end
            if(Select(xpar, 'G', RBFN.R*RBFN.r) > 0)
                i = i + 1;
            end
        end
        if(nxx == nxc)
            RBFN.cen = ParDef.min' + rand(ParDef.M,1).*(ParDef.max-ParDef.min)';
            continue;
        end

        Select(0, 'M', RBFN.R);
        % P R E D I C T I O N    B L O C K
        switch(rem(iLoop,3))
            case 0
                RBFN.fcn = -1;
                st = 'L';
            case 1
                RBFN.fcn = +4;
                st = 'N';
            case 2
                RBFN.fcn = -2;
                st = 'K';
        end
        SetupRBFN();
        [r,p0] = predict(d0);
        RBFN.t_i = RBFN.t_i + toc();
        tic();
        dd0 = eval(sprintf('%s(1, StretchPar(p0));', ParDef.fcn));
        RBFN.t_e = RBFN.t_e + toc();
        tic();
        if(check_data(dd0) ~= 0)
            %dd0 = 1.e+10*ones(Popul.n,1);
            continue;
        end
        nFwd = nFwd + 1;
        [ip,fp] = add_individual(dd0,p0,st,nFwd);
        [ib,fb] = EvalPop();
        % C O R R E C T I O N    B L O C K
        if(r < 1 && nwait) % predicted model is inside the volume spanned by predicting individuals
            RBFN.R = RBFN.R*(0.5+r/2);
            RBFN.R = max(RBFN.R, 1.e-3);
            RBFN.R = min(RBFN.R, Popul.rc);
            if(fp < RBFN.fb)
                st(2) = 'A';
                RBFN.cen = p0;
                nwait = 0;
                %                fprintf(fd_o,'%10.3f  %5d %3d A\n', RBFN.R, Popul.qq, RBFN.fcn);
            else
                if(nwait >= Popul.m)
                    nwait = 0;
                    if(rand() < 0.5)
                        st(2) = 'F';
                        RBFN.cen = ParDef.min' + rand(ParDef.M,1).*(ParDef.max-ParDef.min)';
                    else
                        st(2) = 'H';
                        RBFN.cen = Popul.Par(:,ib);
                    end
                    RBFN.R = Popul.rc;
                    SetupStrategy();
                else
                    st(2) = 'B';
                    RBFN.cen = RBFN.par(:,RBFN.ib);
                    nwait = nwait + 1;
                end
            end
        else      % predicted model is outside the predicting body
            if(fp < RBFN.fb)
                st(2) = 'C';
                RBFN.cen = p0;
                nwait = 0;
                %                fprintf(fd_o,'%10.3f  %5d %3d B\n', RBFN.R, Popul.qq, RBFN.fcn);
            else
                nwait = nwait + 1;
                if(nwait >= Popul.m)
                    nwait = 0;
                    if(rand() < 0.5)
                        st(2) = 'D';
                        RBFN.cen = ParDef.min' + rand(ParDef.M,1).*(ParDef.max-ParDef.min)';
                    else
                        st(2) = 'E';
                        RBFN.cen = Popul.Par(:,ib);
                    end
                    RBFN.R = Popul.rc;
                    SetupStrategy();
                else
                    st(2) = 'X';
                end
            end
        end

        %                 fprintf(1,'R = %10.3f r=%10.3f minRchi = %10.3e pchi = %10.3e bestchi = %10.3e ', RBFN.R, r, min(RBFN.chi), fp, fb);
        %                 fprintf(1,'Best CHI^2 = %10.3e individual # %4.4d fcn = %2d %s %s %5.5d %d\n', fb, ib, RBFN.fcn, Popul.Stat(Popul.ind(1)), st, nFwd, nwait);

        % L O G G I N G    B L O C K
        if(fb < gb)
            switch Popul.Stat(ib)
                case 'N'
                    RBFN.n = RBFN.n + 1;
                case 'L'
                    RBFN.l = RBFN.l + 1;
                case 'K'
                    RBFN.k = RBFN.k + 1;
                case 'G'
                    RBFN.g = RBFN.g + 1;
            end
            gb = fb;
        end
        log_inv([Popul.Stat(Popul.ind(1)) sprintf('%7.3f%7.3f%5d', RBFN.R, RBFN.r, Popul.qq)]);
    end
end
if(ParDef.err == 0)
    x    = Popul.PP;
    fval = Popul.CHI;
    exit_anni();
end

%fclose(fd_o);
% ----------------------------------------------

% S E C T I O N    F O R    I N P U T S

%-----------------------------------------------
    function read_param(in_name)

        ParDef.err = 0;
        open_log('ANNI.log');
        [fd_in,err] = fopen(in_name,'r');
        if(err ~= 0)
            fprintf(1,'Parameter file not accessible\n');
            if(fd_log > 0)
                fprintf(fd_log, 'Parameter file %s not accessible\n',in_name);
                ParDef.err = ParDef.err + 64;
                return;
            end
        end
        j = 1;
        k = 1;
        while(j <= 100)
            inp_txt = fgetl(fd_in);
            if(isempty(inp_txt))
                continue;
            end;
            if(inp_txt == -1)
                if(j == 11)
                    ParDef.msol = 1;
                    ParDef.rsd  = 0;
                    j = 100;
                end
                break;
            end
            lch = inp_txt(1);
            if(lch == '#' || lch == '%')
                continue;
            end;
            switch j
                case 1
                    ParDef.Title = inp_txt;
                    j = j + 1;
                case 2
                    ParDef.n = sscanf(inp_txt, '%d');
                    j = j + 1;
                case 3
                    ParDef.m = sscanf(inp_txt, '%d');
                    j = j + 1;
                case 4
                    ParDef.q = sscanf(inp_txt, '%d');
                    j = j + 1;
                case 5
                    ParDef.max_iter = sscanf(inp_txt, '%d');
                    j = j + 1;
                case 6
                    ParDef.max_time = sscanf(inp_txt, '%d');
                    j = j + 1;
                case 7
                    ParDef.max_nFwd = sscanf(inp_txt, '%d');
                    j = j + 1;
                case 8
                    ParDef.Chi = sscanf(inp_txt, '%f');
                    j = j + 1;
                case 9
                    ParDef.path = sscanf(inp_txt, '%s %*s');
                    ParDef.fcn  = sscanf(inp_txt, '%*s %s');
                    j = j + 1;
                case 10
                    ik = 0;
                    while (~isempty(inp_txt))
                        ik = ik + 1;
                        [token{ik}, inp_txt] = strtok(inp_txt);
                    end

                    if(ik >= 3)
                        ParDef.name{k} = token(1);
                        ParDef.min(k)  = sscanf(char(token(2)), '%f');
                        ParDef.max(k)  = sscanf(char(token(3)), '%f');
                        ParDef.fix(k)  = 0;
                        if(ik >= 4)
                            ParDef.ini(k) = sscanf(char(token(4)), '%f');
                        else
                            ParDef.ini(k) = ParDef.min(k) + (ParDef.max(k)-ParDef.min(k))*rand();
                        end
                        if(ik >= 5)
                            if(char(token(5)) == '*')
                                ParDef.fix(k) = 1;
                            end
                        end
                    else
                        k = k - 1;
                    end
                    k = k + 1;
                    if(k > ParDef.m)
                        j = j + 1;
                    end
                case 11
                    ParDef.msol = sscanf(inp_txt, '%d %*f');
                    [ParDef.rsd, count] = sscanf(inp_txt, '%*d %f');
                    if(count == 0)
                        ParDef.rsd = 1.e-3;
                    end
                    j = 100;
                otherwise
                    break;
            end
        end
        if(j ~= 100)
            ParDef.err = ParDef.err + 1;
        end
        fclose(fd_in);
        head_log();
        if(check_param())
            close_log();
        end
        return
    end

%-----------------------------------------------
    function [err] = check_param()


        if(ParDef.err ~= 1 && fd_log > 0)
            if(ParDef.m < 1)
                fprintf(fd_log, 'Number of parameters = %d (must be > 0)\n', ParDef.m);
                ParDef.err = ParDef.err + 2;
            end
            if(ParDef.n < 1)
                fprintf(fd_log, 'Number of equations = %d (must be > 0)\n', ParDef.n);
                ParDef.err = ParDef.err + 4;
            end
            if(ParDef.q < ParDef.m)
                fprintf(fd_log, 'Population size = %d (must be > %d)\n', ParDef.q, ParDef.m);
                ParDef.err = ParDef.err + 8;
            end
            if(size(ParDef.min,2) ~= ParDef.m || size(ParDef.max,2) ~= ParDef.m)
                fprintf(fd_log, 'Inconsistent number of defined parameters %d - %d (must be %d)\n', ...
                    size(ParDef.min,2), size(ParDef.max,2), ParDef.m);
                ParDef.err = ParDef.err + 16;
            end
            for ii=1:ParDef.m
                if(ParDef.max(ii) < ParDef.min(ii))
                    ParDef.err = ParDef.err + 32;
                    fprintf(fd_log, 'Bad range for parameter %d: <%f %f>\n', ii, ParDef.min(ii), ParDef.max(ii));
                    break;
                end
            end
        end
        ParDef.cen = (ParDef.max + ParDef.min)/2;
        ParDef.rng = (ParDef.max - ParDef.min)/2;
        err = ParDef.err;
        return;

    end

%-----------------------------------------------
    function outpar = StretchPar(inpar)

        if(ParDef.nf == 0)
            outpar = inpar;
            return;
        end
        outpar = ParDef.mm*[inpar; ParDef.inf'];

    end

%-----------------------------------------------
    function ShrinkPar(mod)

        ParDef.nf = sum(ParDef.fix);
        ParDef.M  = ParDef.m;
        if(ParDef.nf == 0)
            return;
        end

        if(mod == 1)
            ParDef.mm = zeros(ParDef.m,ParDef.m);
            ir = 0;
            is = ParDef.m + 1;
            for ii=1:ParDef.m
                if(ParDef.fix(ii) == 0)
                    ir = ir + 1;
                    ParDef.mm(ii,ir) = 1;
                else
                    is = is - 1;
                    ParDef.mm(ii,is) = 1;
                end
            end

            ParDef.M   = ir;
            ParDef.ini = ParDef.ini*ParDef.mm;
            ParDef.min = ParDef.min*ParDef.mm;
            ParDef.max = ParDef.max*ParDef.mm;
            ParDef.cen = ParDef.cen*ParDef.mm;
            ParDef.rng = ParDef.rng*ParDef.mm;

            ParDef.inf = ParDef.ini(:,ParDef.M+1:ParDef.m);
            ParDef.mif = ParDef.min(:,ParDef.M+1:ParDef.m);
            ParDef.maf = ParDef.max(:,ParDef.M+1:ParDef.m);
            ParDef.cef = ParDef.cen(:,ParDef.M+1:ParDef.m);
            ParDef.rnf = ParDef.rng(:,ParDef.M+1:ParDef.m);

            ParDef.ini = ParDef.ini(:,1:ParDef.M);
            ParDef.min = ParDef.min(:,1:ParDef.M);
            ParDef.max = ParDef.max(:,1:ParDef.M);
            ParDef.cen = ParDef.cen(:,1:ParDef.M);
            ParDef.rng = ParDef.rng(:,1:ParDef.M);
        end
        if(mod == -1)
            ParDef.ini = [ParDef.ini ParDef.inf]*ParDef.mm';
            ParDef.min = [ParDef.min ParDef.mif]*ParDef.mm';
            ParDef.max = [ParDef.max ParDef.maf]*ParDef.mm';
            ParDef.cen = [ParDef.cen ParDef.cef]*ParDef.mm';
            ParDef.rng = [ParDef.rng ParDef.rnf]*ParDef.mm';
        end
        return;
    end


% S E C T I O N    F O R    O U T P U T S


%-----------------------------------------------
    function open_log(name)

        t = datevec(now());
        RBFN.t_i = 0;
        RBFN.t_e = 0;
        tic();
        [fd_log, err] = fopen(name,'a+');
        if(err)
            error('Error in opening the protocol file');
        else
            seed = mod(floor(now()*86400),86400); % full seconds since today's midnight
            %          seed = 46907;
            rand('twister', seed);
            randn('state', seed);
            fprintf(fd_log, 'ANNI ver.%s inversion (rand seed = %d)\n', VER, seed);
            fprintf(fd_log, 'Started  %04d/%02d/%02d %02d:%02d:%06.3f\n',t(1),t(2),t(3), t(4),t(5),t(6));
        end
        return

    end

%-----------------------------------------------
    function close_log()

        if(fd_log > 0)
            t = datevec(now());
            RBFN.t_i = RBFN.t_i + toc();
            fprintf(fd_log, 'Total inversion time %.3f s (%.3f internal %.3f external)\n', RBFN.t_i + RBFN.t_e, RBFN.t_i, RBFN.t_e);
            fprintf(fd_log, 'Finished %04d/%02d/%02d %02d:%02d:%06.3f\n',t(1),t(2),t(3), t(4),t(5),t(6));
            fprintf(fd_log, 'Error flag in the end %d\n',ParDef.err);
            if(ParDef.err ~= 0)
                fprintf(fd_log, 'Bit encoding of potential errors:\n');
                fprintf(fd_log, '1,2,4,8,16,32 ... Bad formatting/values in the instruction file\n');
                fprintf(fd_log, '128 ... Inapropriate dimension of returned data vector\n');
                fprintf(fd_log, '256 ... NaN inside user response to trial model\n');
                fprintf(fd_log, '512 ... Inf inside user response to trial model\n');

            end
            %if(exist('Popul.sol','var'))
            fprintf(fd_log, 'Solution Nr. %d (%d)\n', Popul.sol, Popul.nsl(Popul.sol));
            fprintf(fd_log, '***************************\n');
            outpar = StretchPar(Popul.Par(:,Popul.ind(1)));
            for ii=1:ParDef.m
                if(ParDef.fix(ii) == 1)
                    fprintf(fd_log, '%5d *%9s %10.3e\n', ii, char(ParDef.name{ii}), outpar(ii));
                else
                    fprintf(fd_log, '%5d %10s %10.3e\n', ii, char(ParDef.name{ii}), outpar(ii));
                end
            end
            fprintf(fd_log, '***************************\n');
            fprintf(fd_log, 'Norm = %10.3e\n', TrueFit(Popul.ind(1)));
            fprintf(fd_log, '***************************\n');

            fprintf(fd_log, 'Inversion statistics\n');
            nlg = max((RBFN.g+RBFN.l+RBFN.n+RBFN.k)/100, 1/100);
            fprintf(fd_log, 'Number of improvements achieved by random            %d (%4.1f%%)\n', RBFN.g, RBFN.g/nlg);
            fprintf(fd_log, 'Number of improvements achieved by linear regression %d (%4.1f%%)\n', RBFN.l, RBFN.l/nlg);
            fprintf(fd_log, 'Number of improvements achieved by linear prediction %d (%4.1f%%)\n', RBFN.k, RBFN.k/nlg);
            fprintf(fd_log, 'Number of improvements achieved by RBFN prediction   %d (%4.1f%%)\n', RBFN.n, RBFN.n/nlg);
            fprintf(fd_log, 'Total number of function evaluations %d Total number of models used %d Saving ratio %4.1f%%\n', nFwd, Popul.nfw, (Popul.nfw-nFwd)/Popul.nfw*100);
            fprintf(fd_log, '### End of inversion ###\n\n');
            %end
            fclose(fd_log);
            fd_log = -1;
        end
        return;

    end

%-----------------------------------------------
    function head_log()

        if(fd_log > 0)
            fprintf(fd_log, 'Processing the instruction file %s\n', Fname);
            fprintf(fd_log, '%s\n', ParDef.Title);
            fprintf(fd_log, 'Data space dimension  %d\n', ParDef.n);
            fprintf(fd_log, 'Model space dimension %d\n', ParDef.m);
            fprintf(fd_log, 'Population size       %d\n', ParDef.q);
            fprintf(fd_log, 'Number of solutions   %d\n', ParDef.msol);
            fprintf(fd_log, 'Definition of parameters\n');
            fprintf(fd_log, '   Nr       name        min        max\n');
            fprintf(fd_log, '-----+---------+----------+-----------\n');
            for ii=1:ParDef.m
                if(ParDef.fix(ii) == 1)
                    fprintf(fd_log, '%5d %10s %10.3e (fixed)\n', ii, char(ParDef.name{ii}), ParDef.ini(ii));
                else
                    fprintf(fd_log, '%5d %10s %10.3e %10.3e\n', ii, char(ParDef.name{ii}), ParDef.min(ii), ParDef.max(ii));
                end
            end
            fprintf(fd_log, '-----+---------+----------+-----------\n');
            fprintf(fd_log, 'Maximum number of iter: %d\n', ParDef.max_iter);
            fprintf(fd_log, 'Maximum inversion time: %d\n', ParDef.max_time);
            fprintf(fd_log, 'Maximum number of eval: %d\n', ParDef.max_nFwd);
            fprintf(fd_log, 'Chi2 level to reach: %10.3e\n', ParDef.Chi);
            fprintf(fd_log, 'Forward modelling function %s/%s\n', ParDef.path, ParDef.fcn);
        end
        return;

    end

%-----------------------------------------------
    function log_inv(mess)

        if(isempty(mess))
            fprintf(fd_log, 'M------R------r---qq--cycle-ncall------time------misfit\n');
            return;
        end
        itime = RBFN.t_i + RBFN.t_e;
        if(fd_log > 0)
            fprintf(fd_log, '%20s  %5d %5d %9.3f %11.4e\n', mess, iLoop, nFwd, itime, TrueFit(Popul.ind(1)));
        end
        return;

    end


%-----------------------------------------------
    function log_post()

        if(Popul.sol == 1)
            return;
        end

        fd_log = fopen('ANNI.log','a+');
        if(fd_log > 0)
            ltxt = '+--------+';
            for ii=1:Popul.sol
                ltxt = [ltxt  '-----------+'];
            end
            fprintf(fd_log, 'Summary on multiple solutions\n');
            fprintf(fd_log, '%s\n', ltxt);
            fprintf(fd_log, '|        |');
            for ii=1:Popul.sol
                fprintf(fd_log,'   %3d%3d  |', ii, Popul.nsl(ii));
            end
            fprintf(fd_log, '\n%s', ltxt);
            is = 0;
            it = ParDef.nf + 1;
            for im=1:ParDef.m
                if(ParDef.fix(im) == 1)
                    it = it - 1;
                    fprintf(fd_log,'\n*%9s', char(ParDef.name{im}));
                    for ii=1:Popul.sol
                        fprintf(fd_log, ' %11.4e', ParDef.inf(it));
                    end
                else
                    is = is + 1;
                    fprintf(fd_log,'\n%10s', char(ParDef.name{im}));
                    for ii=1:Popul.sol
                        fprintf(fd_log, ' %11.4e', Popul.PP(is,ii));
                    end
                end
            end


            fprintf(fd_log, '\n%s', ltxt);
            fprintf(fd_log, '\nTrue fit  ');
            for ii=1:Popul.sol
                fprintf(fd_log, ' %11.4e', Popul.CHI(ii));
            end
            fprintf(fd_log, '\n\n');

            fprintf(fd_log, 'Number of really different solutions %d\n', Popul.msl);
            pp  = zeros(Popul.m,Popul.msl);
            nsl = zeros(Popul.msl,1);
            ltxt = '+--------+';
            for im=1:Popul.msl
                ltxt = [ltxt  '-----------+'];
                for ii=1:Popul.sol
                    if(Popul.nsl(ii) == im)
                        nsl(im) = nsl(im) + 1;
                        pp(:,im) = pp(:,im) + Popul.PP(:,ii);
                    end
                end
                if(nsl(im) > 0)
                    pp(:,im) = pp(:,im)/nsl(im);
                end
            end

            fprintf(fd_log, '%s\n', ltxt);
            fprintf(fd_log, '|        |');
            for ii=1:Popul.msl
                fprintf(fd_log,'   %6d  |', ii);
            end
            fprintf(fd_log, '\n%s', ltxt);

            is = 0;
            it = ParDef.nf + 1;
            for im=1:ParDef.m
                if(ParDef.fix(im) == 1)
                    it = it - 1;
                    mfprintf(fd_log,'\n*%9s', char(ParDef.name{im}));
                    for ii=1:Popul.msl
                        mfprintf(fd_log, ' %11.4e', ParDef.inf(it));
                    end
                else
                    is = is + 1;
                    fprintf(fd_log,'\n%10s', char(ParDef.name{im}));
                    for ii=1:Popul.msl
                        fprintf(fd_log, ' %11.4e', pp(is,ii));
                    end
                end
            end
            fprintf(fd_log, '\n%s', ltxt);
            fprintf(fd_log, '\nNumber of roots');
            for ii=1:Popul.msl
                fprintf(fd_log, '%4d        ', nsl(ii));
            end
            fprintf(fd_log, '\n\n');
            fclose(fd_log);
            fd_log = -1;
        end
        return;
    end

% I N V E R S I O N    E N G I N E    S E C T I O N

%-----------------------------------------------
% How the structure 'Popul' is designed and used:
% -----------+----------------------------------
% Popul.q   ... total current size of the population
% Popul.qq  ... size of the subpopulation just used for approximation (Popul.qq <= Popul.q)
% Popul.qm  ... maximum allowed size of the population (Popul.qq <= Popul.qm)
% Popul.m   ... dimension of the model space (number of parameters)
% Popul.n   ... dimension of the data space (number of equations)
% Popul.nm  ... number of degree of freedom ( = n-m)
%
% Popul.nf  ... number of radial basis functions used
% Popul.np  ... number of patterns used for learning the RBFN
%
% Popul.Par() ... individuals in the population (model space)
% Popul.Dat() ... individuals in the population (data space)
% Popul.Cp()  ...
% Popul.Cl()  ... Diagonal matrix giving the rule how new individuals are
%                 generated in order to be isotropically distributed along the hypersphere
%
% Popul.Born()... index specifying iteration cycle during which this individual has been created
% Popul.Stat()... character specifying method used for creation of this individual
%                 'I'=random generation during the initialization stage
%                 'N'=RBFN prediction corresponding to zero residuals
%                 'L'=supposing linear problem
%                 'G'=random generation of individuals forming the predicting body
%                 'K'=prediction by Kriging method to zero residuals (currently not used)
% Popul.Mask()... mask defining if the individual will (=1) or will not (=0) be used for approximation
% Popul.QQ()  ... sparse matrix corresponding to Popul.Mask used for fast selection of masked individuals
%                 multiplying by QQ from the left produces selection of the subpopulation from the whole population
%                 selected_par = Popul.par*Popul.QQ
%                 selected_dat = Popul.dat*Popul.QQ;
% Popul.map() ... indices of individuals in the population used for predicting RBFN
%
% Popul.fcn   ... specification of the RBF interpolating function
%                 (1=Gaussian,2=Linear,3=Cauchy,-1=simple linear model)
% Popul.Chi() ... true chi-square of data
% Popul.chi() ... sorted list of Popul.Chi() in increasing order
% Popul.ind() ... list of Popul.Chi() indices to original positions
% Popul.nfw   ...
%
% RBFN.par()  ... selection of Popul.Par used for training the network
% RBFN.dat()  ... selection of Popul.Dat used for training the network
% RBFN.Cd     ... metric tensor ensuring transformation of the RBFN.dat space such that the mean distance between
%                 all the selected vectors <|RBFN.dat(:,i)-RBFN.dat(:,j)|> = 1, i != j
% RBFN.n
% RBFN.l
% RBFN.k
% RBFN.g
%

    function init()

        % xs = ParDef.min + rand(1,ParDef.m,1).*(ParDef.max-ParDef.min);
        % xs = ParDef.min + rand()*(ParDef.max-ParDef.min);
        %rand('seed', 35654);

        RBFN.t_i = RBFN.t_i + toc();
        tic();
        addpath(ParDef.path);
        execstr    = sprintf('%s(0, StretchPar(ParDef.ini''));', ParDef.fcn);
        eval(execstr);
        RBFN.t_e = RBFN.t_e + toc();
        tic();
        nFwd       = 0;
        iLoop      = 0;
        Popul.sol  = 0;
        Popul.q    = 0;
        Popul.qm   = ParDef.M*100;
        Popul.m    = ParDef.M;
        Popul.n    = ParDef.n;
        Popul.nm   = max(Popul.n-Popul.m,1);
        Popul.nfw  = 0;

        Popul.qq   = ParDef.q;
        Popul.nf   = ParDef.q;
        Popul.np   = ParDef.q;
        Popul.fcn  = -1;
        Popul.rc   = sqrt(Popul.m)/2;
        %        Popul.rc   = 1/2;
        Popul.Cl   = ParDef.rng';
        Popul.Cp   = Popul.Cl.^(-1);
        Popul.PP   = [];
        Popul.DD   = [];

        RBFN.dat = [];
        RBFN.par = [];
        RBFN.chi = [];
        RBFN.Cd  = [];
        RBFN.cen = [];
        RBFN.L   = ones(Popul.m,1);
        RBFN.R   = Popul.rc;
        RBFN.fcn = Popul.fcn;
        RBFN.n   = 0;
        RBFN.l   = 0;
        RBFN.k   = 0;
        RBFN.g   = 0;
        RBFN.r   = dist_s2();
        RBFN.b   = 1.5/2;

        Select(0, 'R', RBFN.R);
        RBFN.cen = ParDef.cen';
        ii = 0;
        if(Select(ParDef.ini', 'I', 0) ~= 0)
            ii = ii + 1;
        end
        while(ii ~= ParDef.q)
            xaux = rand(ParDef.M,1)-0.5;
            if(ParDef.M > 1)
                xmag = norm(xaux);
                if(xmag == 0)
                    continue;
                end
                xaux = xaux/norm(xaux);
            end
            xaux = adjust(xaux.*Popul.Cl*RBFN.R+RBFN.cen,RBFN.cen);
            if(Select(xaux, 'I', 0) ~= 0)
                ii = ii + 1;
            end
        end

        Select(0, 'M', RBFN.R);
        return;

    end

%-----------------------------------------------

    function reinit()

        close_log();
        open_log('ANNI.log');
        ShrinkPar(-1);
        head_log();
        ShrinkPar(+1);
        log_inv('');

        RBFN.dat = [];
        RBFN.par = [];
        RBFN.chi = [];
        RBFN.Cd  = [];
        RBFN.cen = ParDef.cen';
        RBFN.L   = ones(Popul.m,1);
        RBFN.R   = Popul.rc;
        RBFN.fcn = Popul.fcn;
        RBFN.n   = 0;
        RBFN.l   = 0;
        RBFN.k   = 0;
        RBFN.g   = 0;
        RBFN.r   = dist_s2();
        RBFN.b   = 1.5/2;
        Popul.qq   = ParDef.q;
        Popul.nf   = ParDef.q;
        Popul.np   = ParDef.q;
        Popul.fcn  = -1;
        Popul.enc  = iLoop;
        Popul.nfw  = 0;
        nrm  = int32(Popul.q/2);
        rind = sort(Popul.ind(1:nrm));
        for ii=nrm:-1:1
            del_individual(rind(ii));
        end

        nFwd     = 0;
        iLoop    = 0;

        return;

    end

%-----------------------------------------------
    function exit_anni()

        close_log();
        log_post();
        clear ParDef;
        clear Popul;
        clear RBFN;
        return;

    end

%-----------------------------------------------
    function [iB, fB] = EvalPop()
        %
        % The current population is classified according the chi^2 misfit. Since the size of the population
        % is not fixed and may have changed during the computations, the current size has to be checked for and
        % here is the only place where the adjustment is done.
        %

        AddPenalty();
        Popul.Chi = diag(Popul.Dat'*Popul.Dat)/Popul.nm + Popul.Pen;
        [Popul.chi,Popul.ind] = sort(Popul.Chi);
        iB = Popul.ind(1);
        fB = Popul.chi(1);
        return;

    end

%-----------------------------------------------
    function p = AddConstraint(p)
        if(Popul.sol > 0)
            w = 1;%max(1-iLoop/Popul.enc, 0);
            if(w > 0)
                dn = zeros(1,Popul.sol);
                ri = dist_p2(p,Popul.PP(:,1:Popul.sol));
                dp = p*ones(1,Popul.sol) - Popul.PP(:,1:Popul.sol);
                for ii=1:Popul.sol
                    dn(ii) = norm(dp(:,ii));
                end
                dp   = dp./repmat(dn, Popul.m,1);
                dpen = sum(repmat(w*exp(-ri/ParDef.rsd),Popul.m,1).*dp, 2);
                p = p + dpen;
            end
        end

    end

%-----------------------------------------------
    function AddPenalty()
        Popul.q = size(Popul.Dat,2);
        if(Popul.sol > 0)
            w = 1;%max(1-iLoop/Popul.enc, 0);
            if(w > 0)
                mpen  = w*mean(Popul.Chi-Popul.Pen);
                Popul.Pen = zeros(Popul.q,1);
                for ii=1:Popul.sol
                    psol = Popul.PP(:,ii);
                    ri = dist_p2(Popul.Par(:,1:Popul.q),psol);
                    dpen = mpen*exp(-ri/ParDef.rsd);
                    Popul.Pen = Popul.Pen + dpen;
                end
            end
        end
        return
    end

%-----------------------------------------------
    function chi = TrueFit(ind)
        %
        % "True fit" of the ind-th individual is computed. "True fit" does
        % not include penalty term.
        %

        if(Popul.sol == 0)
            chi = Popul.Chi(ind);
        else
            chi = Popul.Chi(ind) - Popul.Pen(ind);
        end

        return;

    end


%-----------------------------------------------
    function SetupRBFN()
        %
        % The design matrix H and the matrix of weights W are computed
        % Popul.nf = number of radial basis functions
        % Popul.np = number of learning patterns
        %
        RBFN.par = Popul.Par*Popul.QQ;
        RBFN.dat = Popul.Dat*Popul.QQ;
        RBFN.chi = (Popul.Chi'*Popul.QQ)';
        [chi,ind] = sort(RBFN.chi);
        RBFN.ib = ind(1);
        RBFN.fb = chi(1);
        RBFN.iw = ind(size(ind,1));
        RBFN.fw = chi(size(chi,1));
        clear chi;
        clear ind;

        % Section for linear predictor
        if(RBFN.fcn == -1)
            meanP = mean(RBFN.par, 2);
            meanD = mean(RBFN.dat, 2);
            PC = RBFN.par;
            DC = RBFN.dat;
            PC = PC - meanP*ones(1,Popul.np);
            DC = DC - meanD*ones(1,Popul.np);
            RBFN.H = PC*pinv(DC);
            RBFN.p = meanP - RBFN.H*meanD;
            RBFN.W = [];
            return
        end
        % ---------------------------------
        nd = Popul.np*(Popul.np-1)/2;
        dd = zeros(Popul.n,nd);

        nk = Popul.np-1;
        nl = 1;
        for ii=1:Popul.np-1
            dd(:,nl:nl+nk-1)= RBFN.dat(:,ii)*ones(1, nk)-RBFN.dat(:,ii+1:Popul.np);
            nl = nl + nk;
            nk = nk - 1;
        end

        dd = (dd*dd')*nd/Popul.n;
        [U,S,V] = svd(dd);
        S = diag(S);
        stol = S(1)*1.e-6;
        ind1= find(sign(S-stol)+1);
        ind0= find(sign(S-stol)-1);
        S(ind1) = S(ind1).^(-1);
        S(ind0) = S(ind0)*0;
        RBFN.Cd = diag(sqrt(S))*V';
        clear dd;

        % Section for Kriging predictor
        if(RBFN.fcn == -2)
            RBFN.H = zeros(Popul.np+1, Popul.np+1);
            RBFN.H(Popul.np+1,1:Popul.np) = ones(1,Popul.np);
            for ii=1:Popul.np
                RBFN.H(ii+1:Popul.np,ii) = (r2_rbfn(ii+1:Popul.np,RBFN.dat(:,ii))).^RBFN.b;
            end
            RBFN.H = RBFN.H + RBFN.H';
            return;
        end

        % Section for RBFN predictor
        RBFN.H = eye(Popul.np, Popul.nf)*0.5;
        RBFN.W = zeros(Popul.nf, Popul.m);
        RBFN.P = zeros(Popul.np, Popul.np);
        Ip     = eye(Popul.np,Popul.np);

        switch (RBFN.fcn)
            case 1 % Gaussian
            case 2  % pseudolinear
            case 3  % Cauchy
            case 4  % inverse multiquadric
                RBFN.Cd = RBFN.Cd*Popul.nf;
        end
        for ii=1:Popul.np
            RBFN.H(ii,ii+1:Popul.nf) = h(ii+1:Popul.nf, RBFN.dat(:,ii))';
        end
        RBFN.H = RBFN.H + RBFN.H';
        lamda = mean(RBFN.L);
        H2    = RBFN.H'*RBFN.H;
        [U,S,V] = svd(H2);
        S = diag(S);
        for ii=1:Popul.m
            lam00 = 0.0;
            lnorm = 1.0;
            iter  = 0;
            %W     = RBFN.W(:,ii);
            Hpar  = RBFN.H'*RBFN.par(ii,:)';
            while(iter < 1000 && lnorm > 0.01)
                Sl  = S + lamda;
                stol= max(Sl)*1.e-6;
                ind1= find(sign(Sl-stol)+1);
                ind0= find(sign(Sl-stol)-1);
                Sl(ind1) = Sl(ind1).^(-1);
                Sl(ind0) = Sl(ind0)*0;
                VAR = V*diag(Sl)*U';
                RBFN.P = Ip - RBFN.H*VAR*RBFN.H';
                P2 = RBFN.P*RBFN.P';
                V2 = VAR*VAR;
                W  = VAR*Hpar;
                lamdn  = (W'*VAR*W*trace(RBFN.P));
                if(lamdn == 0)
                    break;
                end
                lamda  = RBFN.par(ii,:)*P2*RBFN.par(ii,:)'*trace(VAR-lamda*V2)/lamdn;
                if(lamda < 0 || lamda > S(1))
                    break;
                end
                RBFN.W(:,ii) = W;
                lnorm  = abs(lamda-lam00)/lamda;
                lam00  = lamda;
                iter = iter + 1;
            end
            RBFN.L(ii)   = lamda;
        end
        return;

    end

%-----------------------------------------------
    function sel_flg = Select(paux, id, r0)
        %
        %
        %
        % Resetting the previous selection
        if(id == 'R')
            Popul.Mask = zeros(1,Popul.q);
            Popul.map  = [];
            Popul.QQ   = [];
            sel_flg    = 0;
            return;
        end

        % Mapping of previously selected individuals
        if(id == 'M')
            in = 0;
            sel_flg = sum(Popul.Mask);
            for ii=1:Popul.q
                if(Popul.Mask(ii) == 0)
                    continue;
                end
                in = in + 1;
                Popul.map(in) = ii;
            end
            Popul.QQ = sparse(Popul.map,(1:Popul.qq),ones(Popul.qq,1),Popul.q,Popul.qq);
            return;
        end

        % Trying to find individual within the current population sufficiently close to xaux
        % and still not selected.
        % This way saving the number of forward modelling could be achieved.
        if(Popul.q > Popul.m)
            dd = sqrt(dist_p2(Popul.Par(:,1:Popul.q),paux));
            dd = max(dd,Popul.Mask'*max(dd));
            [rq, iq] = sort(dd);
            for ii=1:Popul.q
                if(rq(ii) <= r0 && Popul.Mask(iq(ii)) == 0)
                    Popul.Mask(iq(ii)) = 1;
                    sel_flg = iq(ii);
                    Popul.nfw = Popul.nfw + 1;
                    return;
                end
                if(rq(ii) > r0)
                    break;
                end
            end
            clear dd;
        end
        % No individual sufficiently close to the inserted one was found, so forward modelling will be necessary.
        ii = add_individual(NaN, paux, id, nFwd);
        if(Popul.Mask(ii) == 0)
            Popul.Mask(ii) = 1;
            Popul.nfw = Popul.nfw + 1;
            sel_flg = ii;
        else
            sel_flg = 0;
        end
        return;

    end

%-----------------------------------------------
    function SetupStrategy()
        if(rand() < 0.5)
            qq = Popul.m+1 + floor(rand()*(9*Popul.m-1));
        else
            qq = ParDef.q;
        end
        Popul.nf = qq;
        Popul.qq = qq;
        Popul.np = qq;
        if(rand() < 0.5)
            Popul.rc   = sqrt(Popul.m)/2;
        else
            Popul.rc   = 1/2;
        end
        if(rand() < 0.5)
            RBFN.r   = dist_s2();
        else
            RBFN.r = 0;
        end
    end

%-----------------------------------------------
    function rc = dist_s2()
        par = randn(Popul.m,Popul.qq);
        par = par./(repmat(sqrt(diag(par'*par)'),Popul.m,1));
        dx = zeros(Popul.qq-1,1);
        for jj=1:Popul.qq-1
            dp  = repmat(par(:,jj),1,Popul.qq-jj)-par(:,jj+1:Popul.qq);
            dp = dp.^2;
            %        dp = sum(dp,1);
            %        dp = sort(dp);
            %        dx(jj) = dp(1);
            dx(jj) = min(sum(dp,1));
            % disp('OVERIT');
            % pause
        end
        rc = sqrt(mean(dx));
    end

%-----------------------------------------------
    function [d] = dist_p2(p1, p2)
        %
        % Squared distance between the p1 and p2 is computed. Both p1 and p2 can be
        % matrices. Each column of p1 or p2, resp., is interpreted as one
        % particular vector from the parameter space. Popul.Cp scales the unit in such a way, that each edge
        % of the parametric hyperrectangle measures 1.
        %      d(i,j) = dist(p1(:,i) - p2(:,j))
        %

        np1 = size(p1,2);
        np2 = size(p2,2);
        d   = zeros(np1, np2);
        p1  = p1.*repmat(Popul.Cp,1,np1);
        p2  = p2.*repmat(Popul.Cp,1,np2);
        if(np1 < np2)
            for ii=1:np1
                pp = repmat(p1(:,ii),1,np2) - p2;
                d(ii,:) = sum(pp.*pp,1);
            end
        else
            for ii=1:np2
                pp = repmat(p2(:,ii),1,np1) - p1;
                d(:,ii) = sum(pp.*pp,1)';
            end
        end
        clear p1 p2;
        return;

    end

%-----------------------------------------------
    function [r2] = r2_rbfn(ir, rd)

        % Squared distance between the vector d and the centre of the i-th radial basis function.
        % Since the i-th radial basis function is not centered in the i-th individual of the population,
        % mapping is necessary. The RBFN.Cd matrix must be specified in advance. This matrix defines
        % how to compute distances in data space. Metric tensor in data space Md is defined as
        %                        Md = RBFN.Cd'*RBFN.Cd
        % and distance is
        %                      dist(r) = r'*Md*r
        % which is identical to
        %                      dist(r) = r'*RBFN.Cd'*RBFN.Cd*r = (RBFN.Cd*r)'*(RBFN.Cd*r)
        % and the last one equation is coded here.

        dd = rd*ones(1,size(ir,2)) - Popul.Dat(:,Popul.map(ir));
        dd = RBFN.Cd*dd;
        r2 = sum(dd.*dd)';

        return;

    end


%-----------------------------------------------
    function [f] = h(ir, rd)
        %
        % Core implementation of the i-th basis function.
        % r2 ... squared distance of the i-th radial basis function center and the input vector d
        % f  ... contribution of the i-th radial basis function to the interpolation.
        %

        r2 = r2_rbfn(ir, rd);

        switch RBFN.fcn
            case 1
                f = exp(-r2/2);            % Gaussian
            case 2
                f = 1-sqrt(r2);            % piecewise "linear"
            case 3
                f = exp(-sqrt(r2));        % Cauchy function
            case 4
                f = 1./sqrt(1+sqrt(r2));   % Inverse Multiquadric
            otherwise
                f = 0.0;                   % unknown function
        end
        return;

    end

%-----------------------------------------------
    function err = check_data(dd)
        %
        % It can happen that user defined function returns inappropriate
        % result. Such cases are detected here.
        %
        sd  = sort(size(dd));
        if(sd(1) ~= 1 || sd(2) ~= Popul.n)
            ParDef.err = bitor(ParDef.err,128);
        end
        if(sum(isnan(dd)) > 0)
            ParDef.err = bitor(ParDef.err,256);
        end
        if(sum(isinf(dd)) > 0)
            ParDef.err = bitor(ParDef.err,512);
        end
        err = ParDef.err;
        return;
    end

%-----------------------------------------------
    function [r,p] = predict(d)
        %
        % Core function of this code. Interpolating RBFN must be already adjusted. Arbitrary input vector d is
        % fed to the interpolation engine.
        %

        switch RBFN.fcn
            case -1  % pure linear prediction
                p = RBFN.H*d + RBFN.p;
            case -2  % Kriging
                dd = [(r2_rbfn(1:Popul.nf, d)).^RBFN.b; 1];
                p = [RBFN.par zeros(Popul.m,1)]*pinv(RBFN.H)*dd;
                clear dd;
            otherwise          % RBFN prediction
                dd = zeros(Popul.nf,1);
                dd(1:Popul.nf) = h(1:Popul.nf,d);
                p  = RBFN.W'*dd;
                clear dd;
        end
        dc = dist_p2(RBFN.cen, RBFN.par(1:Popul.m,1:Popul.nf));
        p  = adjust(p, RBFN.cen);
        pr = mean(sqrt(dc));
        pp = sqrt(dist_p2(RBFN.cen, p));
        r  = pp/pr;
        clear dc;
        return;

    end

% ----------------------------------------------
    function p = adjust(p,cen)
        %
        % This routine prevents the vector p to leave the allowed hypercube
        %

        %  p = max(ParDef.min,p);
        %  p = min(ParDef.max,p);

        p = AddConstraint(p);
        for ii=1:ParDef.M
            cp = cen - p;
            if(cp(ii) == 0)
                continue;
            end
            if(p(ii) < ParDef.min(ii))
                p = p + cp*(ParDef.min(ii)-p(ii))/cp(ii);
            elseif(p(ii) > ParDef.max(ii))
                p = p + cp*(ParDef.max(ii)-p(ii))/cp(ii);
            end
        end
        return;

    end


% ----------------------------------------------
    function [i,f] = add_individual(Di, Pi, s, n)
        %
        % New individual is conditionally appended at the end of the current population. The necessary
        % condition for accepting this individual is that it must differ from all other still existing
        % individuals within the population (duplicity of individuals is thus restricted).
        % Before inserting the size of the current population is checked.
        %
        %

        % If the inserted individual is identical with any of already present individuals in the population
        % nothing happens
        if(Popul.q > 0)
            dp = Popul.Par - Pi*ones(1,Popul.q);
            dp = sum(dp.*dp,1);
            [fx, ix] = sort(dp);
            if(fx(1) == 0)
                i = ix(1);
                f = Popul.Chi(i);
                return;
            end
        end

        % If the inserted individual was not evaluated, it will be done just now
        if(isnan(Di(1)) == 1)
            RBFN.t_i = RBFN.t_i + toc();
            tic();
            Di = eval(sprintf('%s(1, StretchPar(Pi));', ParDef.fcn));
            if(check_data(Di))
                Di = 1.e+10*ones(Popul.n,1);
            end
            RBFN.t_e = RBFN.t_e + toc();
            tic();
            nFwd = nFwd + 1;
        end

        % If the size of the population should exceed the allowed limit, neccesary number of individuals
        % are removed according their Chi
        ndel = Popul.q - Popul.qm + 1;
        itst = Popul.q;
        while(ndel > 0 && itst > 1)
            idel = Popul.ind(itst);
            if(Popul.Mask(idel) == 0)
                del_individual(idel);
                ndel = ndel - 1;
                itst = itst - 1;
            end
            itst = itst - 1;
        end
        % Finally the individual is inserted into the population
        Popul.q = Popul.q + 1;
        Popul.Dat(:,Popul.q) = Di;
        Popul.Pen(Popul.q,1) = 0;
        Popul.Par(:,Popul.q) = Pi;
        Popul.Born(Popul.q)  = n;
        Popul.Stat(Popul.q)  = s;
        Popul.Mask(Popul.q)  = 0;
        Popul.Chi(Popul.q)   = (Popul.Dat(:,Popul.q)'*Popul.Dat(:,Popul.q))/Popul.nm;
        [Popul.chi,Popul.ind] = sort(Popul.Chi);
        i = Popul.q;
        f = Popul.Chi(i);
        return;

    end


% ----------------------------------------------
    function del_individual(qx)
        % Raw deletion of the qx-th individual from the population. After doing this, many things
        % will not work. All necessary relations have to be set up again. (e.g.
        % Popul.Mask(),Popul.QQ()).

        if(qx < 1 || qx > Popul.q)
            return;
        end
        q1 = qx - 1;
        q2 = qx + 1;
        q3 = Popul.q;

        Popul.Dat  = [Popul.Dat(:,1:q1),Popul.Dat(:,q2:q3)];
        Popul.Pen  = [Popul.Pen(1:q1); Popul.Pen(q2:q3)];
        Popul.Par  = [Popul.Par(:,1:q1),Popul.Par(:,q2:q3)];
        Popul.Born = [Popul.Born(1:q1), Popul.Born(q2:q3)];
        Popul.Stat = [Popul.Stat(1:q1), Popul.Stat(q2:q3)];
        Popul.Mask = [Popul.Mask(1:q1), Popul.Mask(q2:q3)];
        Popul.Chi  = [Popul.Chi(1:q1)',Popul.Chi(q2:q3)']';
        Popul.q = Popul.q - 1;
        [Popul.chi,Popul.ind] = sort(Popul.Chi);

        return

    end


%-----------------------------------------
    function [ret_val] = stop_test()

        iLoop   = iLoop + 1;
        itime   = toc();
        ret_val = 0;
        if(iLoop >= ParDef.max_iter && ParDef.max_iter > 0) % checking number od loops
            log_inv('');
            fprintf(fd_log, 'STOP:::Maximum number of iterations reached\n');
            ret_val = 1;
        end
        if(itime > ParDef.max_time && ParDef.max_time > 0)  % checking elapsed time
            log_inv('');
            fprintf(fd_log, 'STOP:::Computing time exceeded the limit\n');
            ret_val = 2;
        end
        if(nFwd > ParDef.max_nFwd && ParDef.max_nFwd > 0)   % checking number of function evaluations
            log_inv('');
            fprintf(fd_log, 'STOP:::Maximum number of evaluations reached\n');
            ret_val = 3;
        end
        if(TrueFit(Popul.ind(1)) < ParDef.Chi)
            log_inv('');
            fprintf(fd_log, 'STOP:::Final misfit below the significant level\n');
            ret_val = 4;
        end
        if(ParDef.err ~= 0)
            log_inv('');
            fprintf(fd_log, 'STOP:::Run time error detected\n');
            ret_val = 5;
        end

        return;

    end


%-----------------------------------------
    function dsol = IndexSol()
        %
        % Popul.sol .... total number of roots found till now
        % Popul.msl .... total number of different solutions found (is computed here)
        % Popul.nsl[] .. numbers (indices) of all found solutions (may be duplicated)
        %
        % Output: dsol = 0 ... last solution is unique
        %         dsol > 0 ... last solution duplicates some solution already found

        Popul.msl    = 1;
        Popul.nsl(1) = 1;
        for im=2:Popul.sol
            dp2 = dist_p2(Popul.PP(:,im), Popul.PP(:,1:im-1));
            [dp2,dpn] = sort(dp2);
            if(dp2(1) < ParDef.rsd)
                Popul.nsl(im) = Popul.nsl(dpn(1));
            else
                Popul.msl = Popul.msl + 1;
                Popul.nsl(im) = Popul.msl;
            end
        end
        dsol = 0;
        for im=1:Popul.sol-1
            if(Popul.nsl(im) == Popul.nsl(Popul.sol))
                dsol = Popul.nsl(im);
                return;
            end
        end
        return;

    end

end

% -----------------------------------------------------------------------------

